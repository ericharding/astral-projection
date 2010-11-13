using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CSharpQuadTree;
using Astral.Plane;

namespace TileMap
{
	public class MapPane : Canvas
	{
		public int TileWidth { get { return _tileWidth; } set { ResizeTiles(value, _tileHeight); } }
		public int TileHeight { get { return _tileHeight; } set { ResizeTiles(_tileWidth, value); } }
		public TileFactory TileToPlace { set { _tileToPlace = value; _tileToPlacePreview = ((value == null) ? null : new TileCluster(value, new Size(_tileWidth, _tileHeight))); } }
		public bool IsSnapToGrid { get { return _snapToGrid; } set { _snapToGrid = value; this.InvalidateVisual(); } }
		public bool IsDrawGrid { get { return _drawGrid; } set { _drawGrid = value; this.InvalidateVisual(); } }
		public bool HasFileName { get { return !string.IsNullOrEmpty(_mapFileName); } }
		public bool Dirty { get { return _dirty; } }
		public Brush GridBrush { get { return _gridPen.Brush; } set { _gridPen = new Pen(value, 1); this.InvalidateVisual(); } }
		public delegate void TileSizeUpdatedDelegate(int newWidth, int newHeight);
		public event TileSizeUpdatedDelegate OnTileSizeUpdated;

		private const long _origin = 0x7FFFFFFF;
		private Pen _gridPen = new Pen(Brushes.Black, 1);
		private bool _scrolling = false, _hoverTile = false, _leftClick = false, _snapToGrid = true, _drawGrid = true, _dirty = false;
		private Point _mousePos, _mouseHover;
		private long _offsetX = _origin, _offsetY = _origin;
		private int _tileWidth = 50, _tileHeight = 50;
		private string _mapFileName;
		private TileFactory _tileToPlace;
		private TileCluster _tileToPlacePreview;
		private QuadTree<TileCluster> _tiles;
		private Map _map;

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
					PlaceTile(_tileToPlace, _snapToGrid ? FindNearestGridIntersect(e.GetPosition(this)) : e.GetPosition(this), true);
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
				_offsetX += (long) (newPos.X - _mousePos.X);
				_offsetY += (long) (newPos.Y - _mousePos.Y);
				_mousePos = newPos;

				this.InvalidateVisual();
			}

			if (_hoverTile && _tileToPlacePreview != null)
				this.InvalidateVisual();
		}

		private void MapPane_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			_scrolling = false;
		}

		private void MapPane_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			_mousePos = e.GetPosition(this);
			_scrolling = true;
		}

		private void PlaceTile(TileFactory tf, Point where, bool relativeToCanvas)
		{
			TileCluster tile = new TileCluster(tf, new Size(_tileWidth, _tileHeight));
			this.OnTileSizeUpdated += new TileSizeUpdatedDelegate(tile.map_OnTileSizeUpdated);
			tile.Position = relativeToCanvas ? CanvasToReal(where) : where;
			tile.Rotation = _tileToPlacePreview.Rotation;
			tile.Mirror = _tileToPlacePreview.Mirror;
			_tiles.Insert(tile);

			_dirty = true;

			this.InvalidateVisual();
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

		private void ResizeTiles(int newWidth, int newHeight)
		{
			_offsetX = (long)((_offsetX - _origin) * ((double)newWidth / _tileWidth) + _origin);
			_offsetY = (long)((_offsetY - _origin) * ((double)newHeight / _tileHeight) + _origin);

			_tileWidth = newWidth;
			_tileHeight = newHeight;

			if (OnTileSizeUpdated != null)
				OnTileSizeUpdated(_tileWidth, _tileHeight);

			if (_tileToPlacePreview != null)
				_tileToPlacePreview.map_OnTileSizeUpdated(_tileWidth, _tileHeight);

			this.InvalidateVisual();
		}

		public void Clear()
		{
			_tiles = null;
			_map = null;
			GC.Collect();
			_tiles = new QuadTree<TileCluster>(new Size(50, 50), 3, true);
			_map = new Map(_tileWidth, _tileHeight);

			_dirty = false;

			this.InvalidateVisual();
		}

		public void Save()
		{
			this.Save(_mapFileName);
		}

		public void Save(string fileName)
		{
			_map.Save(fileName);

			_dirty = false;
		}

		private void LoadMap(Map map)
		{
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

			from.X = Math.Floor(from.X / _tileWidth) * _tileWidth;
			from.Y = Math.Floor(from.Y / _tileHeight) * _tileHeight;

			return RealToCanvas(from);
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			double w = this.RenderSize.Width, h = this.RenderSize.Height;

			if (_drawGrid)
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

			foreach (TileCluster tc in _tiles.Query(new Rect(_origin - _offsetX, _origin - _offsetY, w, h)))
			{
				Vector offset = new Vector(_origin - _offsetX, _origin - _offsetY);

				tc.Draw(dc, offset);
			}

			if (_hoverTile)
			{
				if (_tileToPlacePreview != null)
				{
					_tileToPlacePreview.Draw(dc, _snapToGrid ? FindNearestGridIntersect(_mouseHover) : _mouseHover, 0.5);
				}
			}
		}
	}
}
