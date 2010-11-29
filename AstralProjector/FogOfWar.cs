﻿using System;
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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Astral.Projector
{
    public class FogOfWar : Control
    {
        IMapDisplay _map;
        WriteableBitmap _fogImage;
        Int32Rect _mapBounds;

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

        public Color FogColor
        {
            get { return (Color)GetValue(FogColorProperty); }
            set { SetValue(FogColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FogColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FogColorProperty =
            DependencyProperty.Register("FogColor", typeof(Color), typeof(FogOfWar), new UIPropertyMetadata(Colors.Black));


        public bool ShowMapBounds { get; set; }
        

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

            UpdateFogBitmap(false);
        }

        private void UpdateFogBitmap()
        {
            UpdateFogBitmap(false);
        }

        private void UpdateFogBitmap(bool copy)
        {
            if (copy)
            {
                throw new NotImplementedException();
            }

            var dims = _map.MapBounds;
            _mapBounds = new Int32Rect((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

            if (_mapBounds.Height > 0 && _mapBounds.Width > 0)
            {
                _fogImage = new WriteableBitmap(_mapBounds.Width, _mapBounds.Height, 96, 96, PixelFormats.Pbgra32, null);
                _fogImage.Fill(this.FogColor);
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
                ClearFogAt(e.GetPosition(this));
            }
        }

        void map_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(this);
                ClearFogAt(position);
            }
        }

        private void ClearFogAt(Point position)
        {
            var viewPort = _map.MapViewport;
            int pixelX = (int)(position.X - viewPort.X - _mapBounds.X);
            int pixelY = (int)(position.Y - viewPort.Y - _mapBounds.Y);


            Color color = Colors.Transparent;
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                color = Colors.Black;
            }

            _fogImage.DrawCircle((int)pixelX, (int)pixelY, _map.TileSize/2, color);
        }

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

            // For debuggin
            if (ShowMapBounds)
            {
                Pen p = new Pen(Brushes.Red, 1.0);
                dc.DrawRectangle(null, p, new Rect(x, y, _mapBounds.Width, _mapBounds.Height));
            }
        }
    }
}
