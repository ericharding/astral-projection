using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSharpQuadTree;
using Astral.Plane;
using System.Collections;
using Astral.Plane.Utility;

namespace TileMap
{
    public class MapPane : Canvas, IMapDisplay
    {
        public int TileWidth { get { return _tileWidth; } set { ResizeTiles(value, _tileHeight); } }
        public int TileHeight { get { return _tileHeight; } set { ResizeTiles(_tileWidth, value); } }
        public int TileSize { get { return Math.Max(_tileWidth, _tileHeight); } set { ResizeTiles(value, value); } }
        public event Action<int> TileSizeChanged;
        public TileFactory TileToPlace { get { return _tileToPlace; } set { _tileToPlace = value; _tileToPlacePreview = ((value == null) ? null : new TileCluster(value, new Size(_tileWidth, _tileHeight))); this.InvalidateVisual(); } }
        public int ActivePlacementLayer { get; set; }
        public bool IsSnapToGrid { get { return _snapToGrid; } set { _snapToGrid = value; this.InvalidateVisual(); } }
        public bool IsDrawGridUnder { get { return _drawGridUnder; } set { _drawGridUnder = value; this.InvalidateVisual(); } }
        public bool IsDrawGridOver { get { return _drawGridOver; } set { _drawGridOver = value; this.InvalidateVisual(); } }
        public bool IsProjectorMode { get { return _projectorMode; } set { _projectorMode = value; this.InvalidateVisual(); } }
        public string FileName { get { return _mapFileName; } private set { _mapFileName = value; FileInfoUpdated(); } }
        public bool Dirty { get { return _dirty; } private set { _dirty = value; FileInfoUpdated(); } }
        public string MapNotes { get { return _map.Notes; } set { _map.Notes = value; _dirty = true; } }
        public Brush GridBrush { get { return _gridPen.Brush; } set { _gridPen = new Pen(value, 1); this.InvalidateVisual(); } }
        public Rect MapBounds { get { return ComputeMapSize(); } }
        public Rect MapViewport { get { return new Rect(_offsetX - _origin, _offsetY - _origin, this.ActualWidth, this.ActualHeight); } }
        public ChangeNotificationWrapper<BitArray, int, bool> LayerMap { get { return _layerMapNotifier; } }
        public event Action<long, long> MapPositionChanged;
        public event Action OnFileInfoUpdated;
        public event Action MapChanged;

        private const long _origin = 0x7FFFFFFF;
        private Pen _gridPen = new Pen(Brushes.Black, 1);
        private bool _scrolling = false, _hoverTile = false, _leftClick = false, _snapToGrid = true, _drawGridUnder = true, _drawGridOver = false, _dirty = false, _projectorMode = false;
        private Point _mousePos, _mouseHover;
        private long _offsetX = _origin, _offsetY = _origin;
        private int _tileWidth = 50, _tileHeight = 50;
        private string _mapFileName;
        private TileFactory _tileToPlace;
        private TileCluster _tileToPlacePreview, _highlightedTile = null;
        private QuadTree<TileCluster> _tiles;
        private Map _map, _library;
        private BitArray _layerMap;
        private ChangeNotificationWrapper<BitArray, int, bool> _layerMapNotifier;

        public MapPane()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            this.ClipToBounds = true;

            this.MouseRightButtonDown += new MouseButtonEventHandler(MapPane_MouseRightButtonDown);
            this.MouseRightButtonUp += new MouseButtonEventHandler(MapPane_MouseRightButtonUp);
            this.MouseMove += new MouseEventHandler(MapPane_MouseMove);
            this.MouseLeave += new MouseEventHandler(MapPane_MouseLeave);
            this.MouseEnter += new MouseEventHandler(MapPane_MouseEnter);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(MapPane_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(MapPane_MouseLeftButtonUp);

            this.Clear();
        }

        private void MapPane_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_leftClick)
            {
                _leftClick = false;

                if (_tileToPlace != null)
                {
                    PlaceTile(_tileToPlace, (_snapToGrid && !_tileToPlace.ArbitraryScale) ? FindNearestGridIntersect(e.GetPosition(this)) : e.GetPosition(this), true, this.ActivePlacementLayer);
                }
            }
        }

        private void MapPane_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _leftClick = true;
        }

        private void MapPane_MouseEnter(object sender, MouseEventArgs e)
        {
            _hoverTile = true;
            this.InvalidateVisual();
        }

        private void MapPane_MouseLeave(object sender, MouseEventArgs e)
        {
            _scrolling = false;
            _hoverTile = false;
            _leftClick = false;
            this.InvalidateVisual();
        }

        private void MapPane_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseHover = e.GetPosition(this);

            if (_scrolling)
            {
                Point newPos = _mouseHover;
                _offsetX += (long)(newPos.X - _mousePos.X);
                _offsetY += (long)(newPos.Y - _mousePos.Y);
                _mousePos = newPos;

                MapPositionUpdated();

                this.InvalidateVisual();
            }

            if (!_projectorMode && _hoverTile && _tileToPlace == null && !_scrolling)
            {
                TileCluster tile = FindTopmostVisibleTileAt(_mouseHover);

                if (_highlightedTile != tile)
                {
                    _highlightedTile = tile;

                    this.InvalidateVisual();
                }
            }

            if (_hoverTile && _tileToPlacePreview != null)
                this.InvalidateVisual();
        }

        private void MapPane_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _scrolling = false;
            Mouse.Capture(null);
        }

        private void MapPane_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mousePos = e.GetPosition(this);
            _scrolling = true;
            Mouse.Capture(this);
        }

        public void SetLibrary(Map library)
        {
            if (library == null)
                return;

            _library = library;
            _map.AddReference(_library);
        }

        private void PlaceTile(TileFactory tf, Point where, bool relativeToCanvas, int layer)
        {
            if (_library == null)
                throw new InvalidOperationException("Use SetLibrary() before PlaceTile()");

            TileCluster tile = new TileCluster(tf, new Size(_tileWidth, _tileHeight));
            tile.Position = relativeToCanvas ? CanvasToReal(where) : where;
            tile.Rotation = _tileToPlacePreview.Rotation;
            tile.Mirror = _tileToPlacePreview.Mirror;
            tile.Scale = _tileToPlace.ArbitraryScale ? _tileToPlacePreview.Scale : 1.0;
            tile.Layer = layer;
            _tiles.Insert(tile);

            _map.AddTile(tile.Tile);

            this.Dirty = true;

            this.InvalidateVisual();
        }

        public void PickUpTile()
        {
            if (!_projectorMode && _highlightedTile != null)
            {
                TileToPlace = _highlightedTile.Tile.Factory;
                _tileToPlacePreview.Tile = _highlightedTile.Tile;

                _tiles.Remove(_highlightedTile);
                _map.RemoveTile(_highlightedTile.Tile);
/*
                Point pos = RealToCanvas(_highlightedTile.Position);
                if ((pos.X - _tileWidth) < 0)
                    SetMapPosition(_offsetX + (long)(_tileWidth - pos.X), _offsetY);
                if ((pos.Y - _tileHeight) < 0)
                    SetMapPosition(_offsetX, _offsetY + (long)(_tileHeight - pos.Y));
                if ((pos.X + _tileWidth) > this.RenderSize.Width)
                    SetMapPosition(_offsetX - (long)(pos.X - this.RenderSize.Width) - _tileWidth, _offsetY);
                if ((pos.Y + _tileHeight) > this.RenderSize.Height)
                    SetMapPosition(_offsetX, _offsetY - (long)(pos.Y - this.RenderSize.Height) - _tileHeight);

                pos = this.PointToScreen(RealToCanvas(_highlightedTile.Position));

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)pos.X, (int)pos.Y);
*/
                _highlightedTile = null;
                this.Dirty = true;

                this.InvalidateVisual();
            }
        }

        public void RotatePreview(bool clockwise)
        {
            if (_tileToPlacePreview != null)
            {
                _tileToPlacePreview.RotateTile(clockwise);
                this.InvalidateVisual();
            }
        }

        public void MirrorPreview(bool horizontal)
        {
            if (_tileToPlacePreview != null)
            {
                _tileToPlacePreview.MirrorTile(horizontal);
                this.InvalidateVisual();
            }
        }

        public void ResizePreview(bool larger)
        {
            if (_tileToPlacePreview != null)
            {
                _tileToPlacePreview.ResizeTile(larger);
                this.InvalidateVisual();
            }
        }

        private void ResizeTiles(int newWidth, int newHeight)
        {
            _offsetX = (long)((_offsetX - _origin) * ((double)newWidth / _tileWidth) + _origin);
            _offsetY = (long)((_offsetY - _origin) * ((double)newHeight / _tileHeight) + _origin);

            _tileWidth = newWidth;
            _tileHeight = newHeight;

            List<TileCluster> temp = new List<TileCluster>();

            foreach (var node in _tiles.GetAllNodes())
                foreach (TileCluster tile in node.Objects)
                    temp.Add(tile);

            foreach (TileCluster tile in temp)
                tile.map_OnTileSizeUpdated(_tileWidth, _tileHeight);

            if (_tileToPlacePreview != null)
                _tileToPlacePreview.map_OnTileSizeUpdated(_tileWidth, _tileHeight);

            _map.TileSizeX = _tileWidth;
            _map.TileSizeY = _tileHeight;

            MapPositionUpdated();

            this.Dirty = true;

            this.InvalidateVisual();

            if (this.TileSizeChanged != null)
            {
                this.TileSizeChanged(this.TileSize);
            }
        }

        public void Clear()
        {
            _tiles = null;
            _map = null;
            GC.Collect();
            _tiles = new QuadTree<TileCluster>(new Size(50, 50), 3, true);
            _map = new Map(_tileWidth, _tileHeight);
            ResetLayerMap();

            this.SetLibrary(_library);

            _offsetX = _offsetY = _origin;
            _tileWidth = _tileHeight = 50;

            if (_tileToPlacePreview != null)
                _tileToPlacePreview.map_OnTileSizeUpdated(_tileWidth, _tileHeight);

            this.FileName = null;
            this.Dirty = false;

            MapUpdated();

            this.InvalidateVisual();
        }

        private void ResetLayerMap(int count=8)
        {
            _layerMap = new BitArray(Math.Max(count, 8), true);
            _layerMapNotifier = new ChangeNotificationWrapper<BitArray, int, bool>(_layerMap);
        }

        public void Save()
        {
            this.Save(_mapFileName);
        }

        public void Save(string fileName)
        {
            this.FileName = fileName;
            _map.Save(fileName);

            this.Dirty = false;
        }

        public void Export(string fileName)
        {
            _map.ExportStandalone(fileName);
        }

        public void SetMap(Map map)
        {
            this.Clear();
            _map = map;
            this.FileName = map.FileName;

            ResetLayerMap(_map.Layers);
            _offsetX = _offsetY = _origin;
            _tileWidth = _map.TileSizeX;
            _tileHeight = _map.TileSizeY;

            foreach (Tile t in _map.Tiles)
            {
                TileCluster tc = new TileCluster(t, new Size(_tileWidth, _tileHeight));
                _tiles.Insert(tc);
            }

            if (_tileToPlacePreview != null)
                _tileToPlacePreview.map_OnTileSizeUpdated(_tileWidth, _tileHeight);

            this.Dirty = false;

            MapUpdated();

            this.InvalidateVisual();
        }

        public void SetMapPosition(long X, long Y)
        {
            if (X < 0 || Y < 0)
                X = Y = _origin;

            _offsetX = X;
            _offsetY = Y;

            MapPositionUpdated();

            this.InvalidateVisual();
        }

        private void MapPositionUpdated()
        {
            if (MapPositionChanged != null)
                MapPositionChanged(_offsetX, _offsetY);
        }

        private void FileInfoUpdated()
        {
            if (OnFileInfoUpdated != null)
                OnFileInfoUpdated();
        }

        private void MapUpdated()
        {
            if (MapChanged != null)
                MapChanged();
        }

        private Rect ComputeMapSize()
        {
            double minX = long.MaxValue, minY = long.MaxValue, maxX = long.MinValue, maxY = long.MinValue;

            foreach (var node in _tiles.GetAllNodes())
            {
                foreach (TileCluster tile in node.Objects)
                {
                    Rect b = tile.Bounds;

                    minX = Math.Min(minX, b.Left);
                    minY = Math.Min(minY, b.Top);
                    maxX = Math.Max(maxX, b.Right);
                    maxY = Math.Max(maxY, b.Bottom);
                }
            }

            double width = maxX - minX;
            double height = maxY - minY;

            if (width >= 0 && height >= 0)
            {
                return new Rect(minX, minY, width, height);
            }

            return default(Rect);
        }

        public BitmapSource GetEntireMapAsBitmap()
        {
            Rect bounds = this.MapBounds;

            if (bounds == default(Rect))
                return null;

            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();

            int w = (int)Math.Ceiling(bounds.Width);
            int h = (int)Math.Ceiling(bounds.Height);

            dc.DrawRectangle(this.Background, null, new Rect(0, 0, w, h));

            DrawTiles(dc, new Vector(-bounds.Left, -bounds.Top), _tiles.Query(bounds), false);

            dc.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Default);
            bmp.Render(dv);

            this.InvalidateVisual();

            return bmp;
        }

        private Point CanvasToReal(Point canvasPoint)
        {
            canvasPoint.Offset(_origin - _offsetX, _origin - _offsetY);
            return canvasPoint;
        }

        private Point RealToCanvas(Point realPoint)
        {
            realPoint.Offset(_offsetX - _origin, _offsetY - _origin);
            return realPoint;
        }

        private Point FindNearestGridIntersect(Point from)
        {
            from = CanvasToReal(from);

            from.X = Math.Floor((from.X + 1) / _tileWidth) * _tileWidth;
            from.Y = Math.Floor((from.Y + 1) / _tileHeight) * _tileHeight;

            return RealToCanvas(from);
        }

        private TileCluster FindTopmostVisibleTileAt(Point where)
        {
            where = CanvasToReal(where);
            List<TileCluster> tiles = _tiles.Query(new Rect(where, new Size(0, 0)));

            tiles.RemoveAll(tc => !_layerMap[tc.Layer]);
            tiles.RemoveAll(tc => !IsPointInRectangle(RealToCanvas(where), tc.RenderCorners[0], tc.RenderCorners[1], tc.RenderCorners[2]));

            if (tiles.Count == 0)
                return null;

            return tiles[tiles.Count - 1];
        }

        private bool IsPointInRectangle(Point p, Point topLeft, Point topRight, Point bottomLeft)
        {
            Vector v0 = Point.Subtract(p, topLeft);
            Vector v1 = Point.Subtract(topRight, topLeft);
            Vector v2 = Point.Subtract(bottomLeft, topLeft);

            double A = Vector.Multiply(v0, v1);
            double B = Vector.Multiply(v1, v1);
            double C = Vector.Multiply(v0, v2);
            double D = Vector.Multiply(v2, v2);

            return (0 <= A && A <= B && 0 <= C && C <= D);
        }

        private void DrawTiles(DrawingContext dc, Vector offset, List<TileCluster> tiles, bool allowHighlight)
        {
            foreach (TileCluster tc in tiles)
            {
                if (_layerMap[tc.Layer])
                {
                    Point where = tc.Position;
                    where.Offset(offset.X, offset.Y);

                    tc.Draw(dc, where, 1.0, allowHighlight && tc == _highlightedTile);
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = this.RenderSize.Width, h = this.RenderSize.Height;

            if (_drawGridUnder)
            {
                DrawGrid(dc, w, h);
            }

            DrawTiles(dc, new Vector(_offsetX - _origin, _offsetY - _origin), _tiles.Query(new Rect(_origin - _offsetX, _origin - _offsetY, w, h)), !_projectorMode);

            if (_drawGridOver)
            {
                DrawGrid(dc, w, h);
            }

            if (_hoverTile)
            {
                if (_tileToPlacePreview != null)
                {
                    Point where = (_snapToGrid && !_tileToPlacePreview.ArbitraryScale) ? FindNearestGridIntersect(_mouseHover) : _mouseHover;

                    _tileToPlacePreview.Draw(dc, where, 0.5);
                }
            }
        }

        private void DrawGrid(DrawingContext dc, double w, double h)
        {
            for (double i = 0.5; i <= (w + _tileWidth); i += _tileWidth)
            {
                double x = (i + (_offsetX - _origin) % _tileWidth);
                dc.DrawLine(_gridPen, new Point(x, 0), new Point(x, h));
            }

            for (double i = 0.5; i <= (h + _tileHeight); i += _tileHeight)
            {
                double y = (i + (_offsetY - _origin) % _tileHeight);
                dc.DrawLine(_gridPen, new Point(0, y), new Point(w, y));
            }
        }
    }
}
