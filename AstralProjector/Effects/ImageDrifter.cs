using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Astral.Projector.Effects
{
    public class ImageDrifter : Control, IDisposable
    {
        int _offsetX = 0;
        int _offsetY = 0;
        DispatcherTimer _dt;
        ImageBrush _imageBrush;

        static ImageDrifter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageDrifter), new FrameworkPropertyMetadata(typeof(ImageDrifter)));
        }

        public ImageDrifter()
        {
            this.Loaded += new RoutedEventHandler(ImageDrifter_Loaded);
            this.DeltaX = 1;
            this.DeltaY = 1;
        }

        void ImageDrifter_Loaded(object sender, RoutedEventArgs e)
        {
            _dt = new DispatcherTimer();
            _dt.Interval = TimeSpan.FromMilliseconds(33);
            _dt.Tick += new EventHandler(_dt_Tick);
            _dt.Start();
        }

        void _dt_Tick(object sender, EventArgs e)
        {
            _offsetY += this.DeltaY;
            _offsetX += this.DeltaX;
            _imageBrush.Viewport = new Rect(_offsetX, _offsetY, 800, 600);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _imageBrush = (ImageBrush)this.GetTemplateChild("_clouds");
            _imageBrush.ImageSource = this.ImageSource;
        }

        public void Dispose()
        {
            _dt.Stop();
        }

        public int DeltaX { get; set; }
        public int DeltaY { get; set; }

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageDrifter), new UIPropertyMetadata(null));

        
    }
}
