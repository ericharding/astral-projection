using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;

namespace Astral.Projector
{
    static class WriteableBitmapEx
    {

        public static void Fill(this WriteableBitmap self, Color color)
        {
            self.Fill(new Int32Rect(0, 0, self.PixelWidth, self.PixelHeight), color);
        }

        public static unsafe void Fill(this WriteableBitmap self, Int32Rect rect, Color color)
        {
            uint col = GetColorBytes(ref color, IsPremultipliedFormat(self));
            ApplyPixelFunction(self, rect, (x, y, p) => col);
        }

        public static unsafe void ApplyPixelFunction(this WriteableBitmap self, Int32Rect rect, Func<int, int, uint, uint> colorFunction)
        {
            if (self == null) return;

            try
            {
                Int32Rect safeRect = new Int32Rect(0, 0, self.PixelWidth, self.PixelHeight);
                int minx = Math.Max(0, rect.X);
                int miny = Math.Max(0, rect.Y);
                int width = Math.Min(self.PixelWidth - minx, rect.Width);
                width = Math.Max(width, 0);
                int height = Math.Min(self.PixelHeight - miny, rect.Height);
                height = Math.Max(height, 0);
                safeRect = new Int32Rect(minx, miny, width, height);

                self.Lock();

                uint* pixels = (uint*)self.BackBuffer;

                for (int x = 0; x < safeRect.Width; x++)
                {
                    for (int y = 0; y < safeRect.Height; y++)
                    {
                        int piX = safeRect.X + x;
                        int piY = safeRect.Y + y;
                        int byteOffset = (piY * self.BackBufferStride) + (piX * 4); // Assuming 32bpp
                        pixels[byteOffset / 4] = colorFunction(x, y, pixels[byteOffset / 4]);
                    }
                }

                self.AddDirtyRect(safeRect);
            }
            finally
            {
                self.Unlock();
            }
        }

        public static void BlitFrom(this WriteableBitmap self, WriteableBitmap other)
        {
            // iterate over the new bitmap and for each pixel find the right pixel in the other image
            try
            {
                self.Lock();
                other.Lock();

                double xRatio = (double)other.PixelWidth / (double)self.PixelWidth;
                double yRatio = (double)other.PixelHeight / (double)self.PixelHeight;

                for (int x = 0; x < self.PixelWidth; x++)
                {
                    for (int y = 0; y < self.PixelHeight; y++)
                    {
                        int sx = (int)(xRatio * x);
                        int sy = (int)(yRatio * y);

                        uint color = other.GetPixel(sx, sy);
                        self.SetPixel(x, y, color);
                    }
                }

                self.AddDirtyRect(new Int32Rect(0, 0, self.PixelWidth, self.PixelHeight));
            }
            finally
            {
                other.Unlock();
                self.Unlock();
            }
        }

        private unsafe static uint GetPixel(this WriteableBitmap self, int x, int y)
        {
            uint* pixels = (uint*)self.BackBuffer;
            int byteoffset = (y * self.BackBufferStride) + (x * 4);
            return pixels[byteoffset / 4];
        }

        private unsafe static void SetPixel(this WriteableBitmap self, int x, int y, uint color)
        {
            uint* pixels = (uint*)self.BackBuffer;
            int byteoffset = (y * self.BackBufferStride) + (x * 4);
            pixels[byteoffset / 4] = color;
        }

        unsafe private static bool IsPremultipliedFormat(WriteableBitmap self)
        {
            return self != null && self.Format == PixelFormats.Pbgra32;
        }

        private static uint GetColorBytes(ref Color color, bool premultiply)
        {
            uint ret;
            if (premultiply)
            {
                double alpha = color.A / 255;
                ret = (uint)color.A << 24;
                uint r = ((uint)(color.R * alpha)) << 16;
                uint g = ((uint)(color.G * alpha)) << 8;
                uint b = (uint)(color.B * alpha);
                ret = ret | r | g | b;
            }
            else
            {
                ret = (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
            }

            return ret;
        }

        public static void CircleAlpha(this WriteableBitmap self, int x0, int y0, int radius, double alpha)
        {
            self.ApplyCircleFunction(x0, y0, radius, (x, y, color) =>
            {
                // a^2+b^2=c^2
                int dx = x0-x;
                int dy = y0-y;
                double distance = Math.Sqrt(dx * dx + dy * dy) / radius;

                uint a = (uint)(alpha * 0xff);
                if (a < 250 && distance > 0.8)
                {
                    double oldAlpha = color >> 24;
                    double max = a;
                    a = (uint)Math.Max(Math.Min(oldAlpha, 5 * 0xff * (distance - 0.8)), a);
                }

                uint ret = (color & 0x00FFFFFF) | (a << 24);
                return ret;
            });
        }

        internal static unsafe void ApplyHorizontalLineFunction(this WriteableBitmap self, int x0, int x1, int y, Func<int, int, uint, uint> operation)
        {
            if (y < 0 || y >= self.PixelHeight) return;

            uint* pixels = (uint*)self.BackBuffer;
            int byteoffsetBase = (y * self.BackBufferStride);
            if (x1 < x0) { int temp = x0; x1=x0; x0=temp; }
            int max = Math.Min(x1, self.PixelWidth);
            for (int x = Math.Max(x0, 0); x < max; x++)
            {
                uint pixelColor = pixels[byteoffsetBase / 4 + x];
                pixels[byteoffsetBase / 4 + x] = operation(x, y, pixelColor);
            }
        }

        public static void ApplyCircleFunction(this WriteableBitmap self, int x0, int y0, int radius, Func<int, int, uint, uint> colorFunction)
        {
            self.Lock();

            int f = 1 - radius;
            int ddF_x = 1;
            int ddF_y = -2 * radius;
            int x = 0;
            int y = radius;

            // Fill pixels in horizontal scan lines (instead of vertical for better locality)

            // center line
            self.ApplyHorizontalLineFunction(x0 - radius, x0 + radius, y0, colorFunction);

            while (x <= y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                self.ApplyHorizontalLineFunction(x0 - x, x0 + x, y0 + y, colorFunction);
                
                self.ApplyHorizontalLineFunction(x0 - x, x0 + x, y0 - y, colorFunction);

                self.ApplyHorizontalLineFunction(x0 - y, x0 + y, y0 + x, colorFunction);

                self.ApplyHorizontalLineFunction(x0 - y, x0 + y, y0 - x, colorFunction);
            }


            int dirtyX = Bound(x0 - radius, self.PixelWidth);
            int dirtyY = Bound(y0 - radius, self.PixelHeight);
            self.AddDirtyRect(new Int32Rect( 
                dirtyX, dirtyY, 
                Bound(radius * 2 + 1, self.PixelWidth - dirtyX),
                Bound(radius * 2 + 1, self.PixelHeight - dirtyY)));
            self.Unlock();
        }

        // If you don't inline this I'm going to hityou c#
        private static int Bound(int value, int max)
        {
            return Math.Max(0, Math.Min(max, value));
        }

        public static WriteableBitmapPixels Pixels(this WriteableBitmap self)
        {
            return new WriteableBitmapPixels(self);
        }
    }

    public class WriteableBitmapPixels
    {
        private WriteableBitmap _bitmap;
        public WriteableBitmapPixels(WriteableBitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public unsafe uint this[int x]
        {
            get
            {
                return ((uint*)_bitmap.BackBuffer)[x];
            }
            set
            {
                ((uint*)_bitmap.BackBuffer)[x] = value;
            }
        }
    }
}
