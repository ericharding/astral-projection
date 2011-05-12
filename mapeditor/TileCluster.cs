using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSharpQuadTree;
using Astral.Plane;

namespace TileMap
{
    public class TileCluster : IQuadObject
    {
        public event EventHandler BoundsChanged;
        public Rect Bounds { get { return _bounds; } }
        private Size TileSize { set { _tileSize = value; UpdateDrawSize(); UpdateBounds(); } }
        public Point Position { get { return _tile.Location; } set { _tile.Location = value; UpdateBounds(); } }
        public int Rotation { get { return _tile.Rotation; } set { _tile.Rotation = value; UpdateDrawSize(); UpdateBounds(); } }
        public TileMirror Mirror { get { return _tile.Mirror; } set { _tile.Mirror = value; UpdateDrawSize(); UpdateBounds(); } }
        public int Layer { get { return _tile.Layer; } internal set { _tile.Layer = value; } }
        public bool ArbitraryScale { get { return _tile.ArbitraryScale; } }
        public double Scale { get { return _tile.Scale; } set { _tile.Scale = value; UpdateDrawSize(); UpdateBounds(); } }
        internal Tile Tile { get { return _tile; } set { _tile = value; UpdateDrawSize(); UpdateBounds(); } }
        internal Point[] RenderCorners { get; private set; }

        private Rect _bounds;
        private Size _drawSize, _tileSize;
        private Vector _borderOffset;
        private Tile _tile;

        public TileCluster(TileFactory tf, Size tileSize)
        {
            _tile = tf.CreateTile();
            TileSize = tileSize;
        }

        public TileCluster(Tile tile, Size tileSize)
        {
            _tile = tile;
            TileSize = tileSize;
        }

        public void map_OnTileSizeUpdated(int newWidth, int newHeight)
        {
            _tile.Location = new Point(_tile.Location.X * ((double)newWidth / _tileSize.Width), _tile.Location.Y * ((double)newHeight / _tileSize.Height));
            TileSize = new Size(newWidth, newHeight);
        }

        private const int LEFTBORDER = 3, TOPBORDER = 2, RIGHTBORDER = 1, BOTTOMBORDER = 0;
        private double GetBorder(int side, bool corrected)
        {
            if (corrected)
            {
                if ((_tile.Mirror & TileMirror.Horizontal) == TileMirror.Horizontal)
                {
                    if (side == LEFTBORDER || side == RIGHTBORDER)
                        side += 2;
                }
                if ((_tile.Mirror & TileMirror.Vertical) == TileMirror.Vertical)
                {
                    if (side == TOPBORDER || side == BOTTOMBORDER)
                        side += 2;
                }

                side += (_tile.Rotation / 90);
                side %= 4;
            }

            switch (side)
            {
                case LEFTBORDER: return _tile.Borders.Left;
                case TOPBORDER: return _tile.Borders.Top;
                case RIGHTBORDER: return _tile.Borders.Right;
                case BOTTOMBORDER: return _tile.Borders.Bottom;
                default: return 0;
            }
        }

        private Size GetCorrectedSize()
        {
            if (_tile.ArbitraryScale)
                return _drawSize;

            if (_tile.Rotation == 90 || _tile.Rotation == 270)
                return new Size(_drawSize.Height, _drawSize.Width);
            else
                return _drawSize;
        }

        private void UpdateDrawSize()
        {
            if (_tile.ArbitraryScale)
            {
                double w = _tile.Image.PixelWidth, h = _tile.Image.PixelHeight;
                double min = Math.Min(w, h);

                _drawSize = new Size(w * (_tileSize.Width / min) * _tile.Scale, h * (_tileSize.Height / min) * _tile.Scale);

                _borderOffset = new Vector(0, 0);

                return;
            }

            Size newSize = new Size();

            double imgWidth = _tile.Image.PixelWidth - GetBorder(LEFTBORDER, false) - GetBorder(RIGHTBORDER, false);
            double imgHeight = _tile.Image.PixelHeight - GetBorder(TOPBORDER, false) - GetBorder(BOTTOMBORDER, false);
            double width = _tile.TilesHorizontal * _tileSize.Width;
            double height = _tile.TilesVertical * _tileSize.Height;
            double offsetL = (width * GetBorder(LEFTBORDER, false)) / imgWidth;
            double offsetR = (width * GetBorder(RIGHTBORDER, false)) / imgWidth;
            double offsetT = (height * GetBorder(TOPBORDER, false)) / imgHeight;
            double offsetB = (height * GetBorder(BOTTOMBORDER, false)) / imgHeight;
            double offsetL_c = (width * GetBorder(LEFTBORDER, true)) / imgWidth;
            double offsetT_c = (height * GetBorder(TOPBORDER, true)) / imgHeight;
            newSize.Width = width + offsetL + offsetR;
            newSize.Height = height + offsetT + offsetB;

            _drawSize = newSize;

            _borderOffset = new Vector(offsetL_c, offsetT_c);
        }

        private void UpdateBounds()
        {
            if (_tile.ArbitraryScale)
            {
                Point where = _tile.Location;

                double centerX = _drawSize.Width / 2;
                double centerY = _drawSize.Height / 2;

                where.Offset(-centerX, -centerY);

                Rect tileRect = new Rect(where, _drawSize);

                RectangleGeometry rg = new RectangleGeometry(tileRect);

                rg.Transform = new RotateTransform(_tile.Rotation, where.X + centerX, where.Y + centerY);

                _bounds = rg.Bounds;
            }
            else
            {
                Point realPos = new Point(_tile.Location.X - _borderOffset.X, _tile.Location.Y - _borderOffset.Y);
                _bounds = new Rect(realPos, GetCorrectedSize());
            }

            if (BoundsChanged != null)
                BoundsChanged(this, new EventArgs());
        }

        public void RotateTile(bool clockwise)
        {
            if (((_tile.Mirror & TileMirror.Horizontal) == TileMirror.Horizontal) ^
                ((_tile.Mirror & TileMirror.Vertical) == TileMirror.Vertical))
                clockwise = !clockwise;

            int inc = _tile.ArbitraryScale ? 5 : 90;
            int max = _tile.ArbitraryScale ? 355 : 270;

            int newRot;

            if (clockwise)
                newRot = (_tile.Rotation + inc) % 360;
            else
            {
                newRot = _tile.Rotation - inc;
                newRot = newRot < 0 ? max : newRot;
            }

            _tile.Rotation = newRot;

            UpdateDrawSize();
            UpdateBounds();
        }

        public void MirrorTile(bool horizontal)
        {
            if (horizontal)
                _tile.Mirror ^= TileMirror.Horizontal;
            else
                _tile.Mirror ^= TileMirror.Vertical;

            UpdateDrawSize();
            UpdateBounds();
        }

        public void ResizeTile(bool larger)
        {
            if (_tile.ArbitraryScale)
            {
                double inc = larger ? 0.05 : -0.05;

                if (_tile.Scale + inc > 0.0)
                {
                    _tile.Scale += inc;

                    UpdateDrawSize();
                    UpdateBounds();
                }
            }
        }

        public void Draw(DrawingContext dc, Point where, double opacity = 1.0, bool highlight = false)
        {
            int mirrorX = 1, mirrorY = 1;

            if ((_tile.Mirror & TileMirror.Horizontal) == TileMirror.Horizontal)
                mirrorX = -1;

            if ((_tile.Mirror & TileMirror.Vertical) == TileMirror.Vertical)
                mirrorY = -1;

            where.Offset(-_borderOffset.X, -_borderOffset.Y);

            if (_tile.ArbitraryScale)
                where.Offset(-_drawSize.Width / 2, -_drawSize.Height / 2);

            Rect tileRect = new Rect(where, _drawSize);

            RectangleGeometry rg = new RectangleGeometry(tileRect);

            double centerX = _drawSize.Width / 2;
            double centerY = _drawSize.Height / 2;

            ScaleTransform scaleXform = new ScaleTransform(mirrorX, mirrorY, where.X + centerX, where.Y + centerY);
            RotateTransform rotXform = new RotateTransform(_tile.Rotation, where.X + centerX, where.Y + centerY);
            TransformGroup xform = new TransformGroup();
            xform.Children.Add(rotXform);
            xform.Children.Add(scaleXform);
            rg.Transform = xform;

            Rect wrong = rg.Rect;
            Rect right = rg.Bounds;
            TranslateTransform transXform = new TranslateTransform(wrong.X - right.X, wrong.Y - right.Y);

            if (!_tile.ArbitraryScale)
                xform.Children.Add(transXform);

            Point[] corners = { rg.Rect.TopLeft, rg.Rect.TopRight, rg.Rect.BottomLeft, rg.Rect.BottomRight };
            xform.Value.Transform(corners);
            RenderCorners = corners;

            dc.PushTransform(xform);
            dc.PushOpacity(opacity);
            dc.DrawImage(_tile.Image, tileRect);
            if (highlight)
            {
                dc.PushOpacity(0.5);
                dc.DrawRectangle(Brushes.Goldenrod, null, tileRect);
                dc.Pop();
            }
            dc.Pop();
            dc.Pop();
        }
    }
}
