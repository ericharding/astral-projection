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

namespace TileMap
{
    /// <summary>
    /// Interaction logic for Minimap.xaml
    /// </summary>
    public partial class Minimap : UserControl
    {
        public MapPane Map
        {
            set
            {
                if (_map != null)
                {
                    _map.BitmapChanged -= Update;
                    _map.MapViewportChanged -= UpdateOutline;
                }

                _map = value;

                if (_map != null)
                {
                    _map.BitmapChanged += Update;
                    _map.MapViewportChanged += UpdateOutline;
                }

                Update();
            }
        }

        private MapPane _map;
        private VisualBrush _brush;
        private bool _scrolling = false;

        public Minimap()
        {
            InitializeComponent();

            this.ClipToBounds = true;

            this.MouseDown += new MouseButtonEventHandler(Minimap_MouseDown);
            this.MouseUp += new MouseButtonEventHandler(Minimap_MouseUp);
            this.MouseMove += new MouseEventHandler(Minimap_MouseMove);
            this.MouseLeave += new MouseEventHandler(Minimap_MouseLeave);

            _brush = new VisualBrush();
            _brush.Stretch = Stretch.Uniform;
            canvasMap.Background = _brush;
        }

        private void Minimap_MouseLeave(object sender, MouseEventArgs e)
        {
            _scrolling = false;
        }

        private void Minimap_MouseMove(object sender, MouseEventArgs e)
        {
            if (_scrolling)
            {
                UpdatePosition(e);
            }
        }

        private void Minimap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_scrolling)
                UpdatePosition(e);

            _scrolling = false;
            Mouse.Capture(null);
        }

        private void Minimap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _scrolling = true;

            UpdatePosition(e);
            Mouse.Capture(this);
        }
/*
        public static Rect SuperRect(Rect a, Rect b)
        {
            double minX = Math.Min(a.Left, b.Left);
            double minY = Math.Min(a.Top, b.Top);
            double maxX = Math.Max(a.Right, b.Right);
            double maxY = Math.Max(a.Bottom, b.Bottom);

            double width = maxX - minX;
            double height = maxY - minY;

            if (width >= 0 && height >= 0)
                return new Rect(minX, minY, width, height);
            else
                return default(Rect);
        }
*/
        private void Update()
        {
            if (_map != null)
            {
                int w, h;

                _brush.Visual = _map.GetEntireMapAsVisual(out w, out h);

                canvasMap.Width = w;
                canvasMap.Height = h;

                UpdateOutline();
            }
        }

        private void UpdateOutline()
        {
            if (_map != null)
            {
                overlayBounds.MapArea = _map.MapBounds;
                overlayBounds.VisibleArea = _map.MapViewport;
                overlayBounds.InvalidateVisual();
            }
        }

        private void UpdatePosition(MouseEventArgs e)
        {
            if (_map != null)
            {
                Point center = e.GetPosition(canvasMap);
                Rect bounds = _map.MapBounds;
                Rect vis = _map.MapViewport;

                long newX = (long)((center.X + bounds.Left) - (vis.Width / 2));
                long newY = (long)((center.Y + bounds.Top) - (vis.Height / 2));

                _map.SetMapPosition(-newX, -newY);
            }
        }
    }

    internal class OverlayBounds : Control
    {
        public Rect MapArea { get; set; }
        public Rect VisibleArea { get; set; }

        private Pen _pen;

        public OverlayBounds()
        {
            this.Background = Brushes.Transparent;
            _pen = new Pen(Brushes.Blue, 1.0);
            _pen.DashStyle = DashStyles.Solid;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = this.RenderSize.Width,
                   h = this.RenderSize.Height;

            double x = -VisibleArea.Left - MapArea.Left;
            double y = -VisibleArea.Top - MapArea.Top;
            double ratioX = w / MapArea.Width;
            double ratioY = h / MapArea.Height;

            dc.DrawRectangle(null, _pen, new Rect(x * ratioX, y * ratioY, VisibleArea.Width * ratioX, VisibleArea.Height * ratioY));
        }
    }
}
