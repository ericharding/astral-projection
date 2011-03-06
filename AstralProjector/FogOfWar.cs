using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Astral.Plane;
using System.Windows.Threading;

namespace Astral.Projector
{
    public class FogOfWar : Control
    {
        IMapDisplay _map;
        WriteableBitmap _fogImage;
        Int32Rect _mapBounds;
        bool _fogDirty = true;
        bool _copyFog = false;

        static FogOfWar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FogOfWar), new FrameworkPropertyMetadata(typeof(FogOfWar)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public IMapDisplay MapDisplay
        {
            get { return (Astral.Plane.IMapDisplay)GetValue(MapDisplayProperty); }
            set { SetValue(MapDisplayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MapDisplay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapDisplayProperty =
            DependencyProperty.Register("MapDisplay", typeof(Astral.Plane.IMapDisplay), typeof(FogOfWar), new UIPropertyMetadata(new PropertyChangedCallback(MapDisplayChanged)));

        private static void MapDisplayChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            // map changed
            FogOfWar self = (FogOfWar)sender;
            self.SetMapDisplay((IMapDisplay)args.NewValue);
        }

        public Color FogColor
        {
            get { return (Color)GetValue(FogColorProperty); }
            set { SetValue(FogColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FogColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FogColorProperty =
            DependencyProperty.Register("FogColor", typeof(Color), typeof(FogOfWar), new UIPropertyMetadata(Colors.Black));

        public void SetMapDisplay(IMapDisplay newMap)
        {
            FrameworkElement feOld = _map as FrameworkElement;
            if (feOld != null)
            {
                feOld.MouseDown -= new MouseButtonEventHandler(map_PreviewMouseDown);
                feOld.MouseMove -= new MouseEventHandler(map_PreviewMouseMove);
                _map.MapChanged -= UpdateFogBitmap;
                _map.MapPositionChanged -= _map_MapPositionChanged;
                _map.TileSizeChanged -= _map_TileSizeChanged;
            }

            _map = newMap;
            FrameworkElement map = _map as FrameworkElement;
            if (map != null)
            {
                map.MouseDown += new MouseButtonEventHandler(map_PreviewMouseDown);
                map.MouseMove += new MouseEventHandler(map_PreviewMouseMove);
            }
            _map.MapChanged += UpdateFogBitmap;
            _map.MapPositionChanged += _map_MapPositionChanged;
            _map.TileSizeChanged += _map_TileSizeChanged;

            UpdateFogBitmap(false);
        }

        void _map_TileSizeChanged(int obj)
        {
            UpdateFogBitmap(true, false);
        }

        private void UpdateFogBitmap()
        {
            UpdateFogBitmap(false);
        }

        private void UpdateFogBitmap(bool copy, bool now = false)
        {
            _fogDirty = true;
            _copyFog = copy;
            this.Dispatcher.In(TimeSpan.FromSeconds(0.5), UpdateFogNow);
        }

        private void UpdateFogNow()
        {
            if (!_fogDirty) return;
            _fogDirty = false;

            var dims = _map.MapBounds;
            _mapBounds = new Int32Rect((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

            if (_mapBounds.Height > 0 && _mapBounds.Width > 0)
            {
                WriteableBitmap oldBitmap = _fogImage;

                _fogImage = new WriteableBitmap(_mapBounds.Width, _mapBounds.Height, 96, 96, PixelFormats.Pbgra32, null);
                if (_copyFog && oldBitmap != null)
                {
                    _fogImage.BlitFrom(oldBitmap);
                }
                else
                {
                    _fogImage.Fill(this.FogColor);
                }
                this.InvalidateVisual();
            }
        }

        void _map_MapPositionChanged(long arg1, long arg2)
        {
            this.InvalidateVisual();
        }

        void map_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(this);
                ChangeFogInternal((int)position.X, (int)position.Y, !Keyboard.IsKeyDown(Key.LeftShift));
            }
        }

        void map_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(this);
                ChangeFogInternal(position.X, position.Y, !Keyboard.IsKeyDown(Key.LeftShift));
            }
        }

        private void ChangeFogInternal(double x, double y, bool clear)
        {
            var viewPort = _map.MapViewport;
            double pixelX = x - viewPort.X - _mapBounds.X;
            double pixelY = y - viewPort.Y - _mapBounds.Y;

            UpdateFogNow();

            // pixelX, pixelY is the pixel on the fog that was clicked
            if (_fogImage != null)
            {
                ChangeFog(pixelX / _fogImage.PixelWidth, pixelY / _fogImage.PixelHeight, clear);
            }
        }

        public void ChangeFog(double x, double y, bool clear)
        {
            int multiplier = Keyboard.IsKeyDown(Key.LeftCtrl) ? 1 : 5;
            ChangeFog(x, y, _map.TileSize * multiplier, clear);
        }

        public void ChangeFog(double x, double y, int size, bool clear)
        {
            UpdateFogNow();

            // Translate from percentage to pixel
            int pixelX = (int)(_fogImage.PixelWidth * x);
            int pixelY = (int)(_fogImage.PixelHeight * y);

            _fogImage.CircleAlpha(pixelX, pixelY, size / 2, clear);

            if (this.FogChanged != null)
            {
                FogChanged(x, y, size, clear);
            }
        }

        public event Action<double, double, int, bool> FogChanged;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_map == null || _fogImage == null) return;

            var viewport = _map.MapViewport;
            double x = _mapBounds.X + viewport.X;
            double y = _mapBounds.Y + viewport.Y;

            // Draw the fog bitmap
            dc.DrawImage(_fogImage, new Rect(x, y, _fogImage.PixelWidth, _fogImage.PixelHeight));

            // Draw a FogColor rectangle around all areas not covered by fog
            SolidColorBrush borderBrush = new SolidColorBrush(this.FogColor);

            dc.DrawRectangle(borderBrush, null, new Rect(0, 0, Math.Max(x, 0), this.ActualHeight));
            dc.DrawRectangle(borderBrush, null, new Rect(0, 0, this.ActualWidth, Math.Max(y, 0)));
            double fogEndRight = x + _fogImage.PixelWidth;
            dc.DrawRectangle(borderBrush, null, new Rect(fogEndRight, 0, Math.Max(0, this.ActualWidth - fogEndRight), this.ActualHeight));
            double fogendBottom = y + _fogImage.PixelHeight;
            dc.DrawRectangle(borderBrush, null, new Rect(0, fogendBottom, this.ActualWidth, Math.Max(0, this.ActualHeight - fogendBottom)));
        }

        internal void Reset()
        {
            if (_fogImage != null)
            {
                _fogImage.Fill(this.FogColor);
            }
        }
    }
}
