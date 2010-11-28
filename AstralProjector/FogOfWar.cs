using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Astral.Plane;
using Astral.Plane.Utility;

namespace Astral.Projector
{
    public class FogOfWar : Control
    {
        IMapDisplay _map;
        WriteableBitmap _fogImage;
        Rect _mapDims;

        static FogOfWar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FogOfWar), new FrameworkPropertyMetadata(typeof(FogOfWar)));
        }


        public FogOfWar()
        {
            Console.WriteLine("testing?");
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

        public void SetMapDisplay(IMapDisplay newMap)
        {
            FrameworkElement feOld = _map as FrameworkElement;
            if (feOld != null)
            {
                feOld.MouseDown -= new MouseButtonEventHandler(map_PreviewMouseDown);
                feOld.MouseMove -= new MouseEventHandler(map_PreviewMouseMove);
                _map.MapChanged -= UpdateFogBitmap;
            }

            _map = newMap;
            FrameworkElement map = _map as FrameworkElement;
            if (map != null)
            {
                map.MouseDown += new MouseButtonEventHandler(map_PreviewMouseDown);
                map.MouseMove += new MouseEventHandler(map_PreviewMouseMove);
            }
            _map.MapChanged += UpdateFogBitmap;
            _map.MapPositionChanged += new Action<long, long>(_map_MapPositionChanged);

            UpdateFogBitmap();
        }

        private void UpdateFogBitmap()
        {
            _mapDims = _map.MapBounds;

            if (_mapDims.Height > 0 && _mapDims.Width > 0)
            {
                // For now lets just make the fog big
                // Then use all the clicks as percentage
                _fogImage = new WriteableBitmap(4096 * 2, 4096 * 2, 96, 96, PixelFormats.Pbgra32, null);
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
            }
        }

        void map_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point clickPoint = e.GetPosition(this);
                Point mapPoint = new Point(clickPoint.X + _map.MapPositionX, clickPoint.Y + _map.MapPositionY);
            }
        }

        public Point PixelToMapCoord(Point pixel)
        {
            return new Point(TranslateX(pixel.X), TranslateY(pixel.Y));
        }

        public double TranslateX(double x)
        {
            throw new NotImplementedException();
        }

        public double TranslateY(double y)
        {
            throw new NotImplementedException();
        }

        protected override void OnRender(DrawingContext dc)
        {
            Pen p = new Pen(Brushes.Black, 2);
            dc.DrawRectangle(Brushes.Red, p, new Rect(100, 100, 200, 200));



            base.OnRender(dc);
        }
    }
}
