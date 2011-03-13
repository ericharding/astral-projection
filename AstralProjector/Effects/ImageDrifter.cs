using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Astral.Projector.Effects
{
    public class ImageDrifter : Control, IDisposable
    {
        int _deltaX = 1;
        int _deltaY = 1;
        int _fps = 30;
        int _width, _height;
        ImageBrush _imageBrush;
        BitmapSource _imageSource;
        RectAnimation _rectAnim;
        Color? _tint;
        bool _loaded = false;

        static ImageDrifter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageDrifter), new FrameworkPropertyMetadata(typeof(ImageDrifter)));
        }

        public ImageDrifter()
        {
            this.Loaded += new RoutedEventHandler(ImageDrifter_Loaded);
        }

        void ImageDrifter_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
            ResetAnimation();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _imageBrush = (ImageBrush)this.GetTemplateChild("_clouds");

            ResetAnimation();
        }

        private void ResetAnimation()
        {
            if (!_loaded)
                return;

            if (_tint != null)
                _imageBrush.ImageSource = Colorize(this.ImageSource, (Color)_tint);
            else
                _imageBrush.ImageSource = this.ImageSource;

            _width = this.ImageSource.PixelWidth;
            _height = this.ImageSource.PixelHeight;

            int x = _deltaX * _width;
            int y = _deltaY * _height;

            Vector vTotal = new Vector(x, y);
            Vector vStep = new Vector(_deltaX, _deltaY);

            Duration dur = new Duration(TimeSpan.FromSeconds((1f / _fps) * (vTotal.Length / vStep.Length)));

            Rect from = new Rect(0, 0, _width, _height);
            Rect to = new Rect(x, y, _width, _height);

            _rectAnim = new RectAnimation(from, to, dur);
            _rectAnim.RepeatBehavior = RepeatBehavior.Forever;

            _imageBrush.BeginAnimation(ImageBrush.ViewportProperty, _rectAnim);
        }

        private static BitmapSource Colorize(BitmapSource bmp, Color color)
        {
            WriteableBitmap wbmp = new WriteableBitmap(bmp);

            WriteableBitmapPixels pixels = new WriteableBitmapPixels(wbmp);

            int w = wbmp.PixelWidth,
                h = wbmp.PixelHeight;

            uint[] colorizedPixels = new uint[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = x + (y * w);

                    colorizedPixels[i] = (pixels[i] & 0xFF000000) | (uint)((color.R << 16) | (color.G << 8) | color.B);
                }
            }

            wbmp.WritePixels(new Int32Rect(0, 0, w, h), colorizedPixels, wbmp.BackBufferStride, 0);

            return wbmp;
        }

        public void Dispose()
        {
        }

        public int DeltaX
        {
            get { return _deltaX; }
            set { _deltaX = value; ResetAnimation(); }
        }

        public int DeltaY
        {
            get { return _deltaY; }
            set { _deltaY = value; ResetAnimation(); }
        }

        public int FPS
        {
            get { return _fps; }
            set { _fps = value; ResetAnimation(); }
        }

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; ResetAnimation(); }
        }

        /// <summary>
        /// Gets or sets a Color to tint the image.
        /// When set, this treats ImageSource as an alpha mask.
        /// </summary>
        public Color? Tint
        {
            get { return _tint; }
            set { _tint = value; ResetAnimation(); }
        }
    }
}
