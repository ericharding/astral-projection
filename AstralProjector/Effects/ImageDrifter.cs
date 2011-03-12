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
        ImageSource _imageSource;
        RectAnimation _rectAnim;
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

            _imageBrush.ImageSource = this.ImageSource;
            _width = ((BitmapSource)this.ImageSource).PixelWidth;
            _height = ((BitmapSource)this.ImageSource).PixelHeight;

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

        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; ResetAnimation(); }
        }
    }
}
