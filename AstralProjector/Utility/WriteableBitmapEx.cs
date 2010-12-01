using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

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
                        pixels[byteOffset / 4] = colorFunction(x, y, pixels[byteOffset/4]);
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

            other.Unlock();
            self.Unlock();
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

        public static unsafe void SetPixel(this WriteableBitmap self, int x, int y, Color color)
        {
            uint col = GetColorBytes(ref color, IsPremultipliedFormat(self));
            int target = (x * self.BackBufferStride) + (y * 4);
            ((uint*)self.BackBuffer)[target] = col;
        }

        public static unsafe void DrawCircle(this WriteableBitmap self, int x, int y, double radius, Color color)
        {
            // todo: approximate circle better
            self.Fill(new Int32Rect((int)(x - radius), (int)(y - radius), (int)(radius * 2), (int)(radius * 2)), color);

            //self.ApplyFunction(new Int32Rect((int)(x - radius), (int)(y - radius), (int)(radius * 2), (int)(radius * 2)), (p) => 
            //{
            //    uint alpha = p >> 24;
            //    alpha /= 2;
            //    alpha = alpha << 24;
            //    return (p & 0x00FFFFFF) | alpha;
            //});
        }
    }
}
