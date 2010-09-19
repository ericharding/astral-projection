﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CSharpQuadTree;

namespace TileMap
{
	public class MapPane : Canvas
	{
		public int TileWidth { get; set; }
		public int TileHeight { get; set; }
		public TileCluster TileToPlace { get; set; }

		private const long _origin = 0x7FFFFFFF;
		private Pen _gridPen;
		private bool _scrolling = false, _hoverTile = false, _leftClick = false;
		private Point _mousePos, _mouseHover;
		private long _offsetX = _origin, _offsetY = _origin;
		private QuadTree<TileCluster> _tiles = new QuadTree<TileCluster>(new Size(50, 50), 3, true);

		public MapPane()
		{
			RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

			_gridPen = new Pen(Brushes.Black, 1);
			_gridPen.DashStyle = DashStyles.Solid;

			this.MouseRightButtonDown += new MouseButtonEventHandler(MapPane_MouseRightButtonDown);
			this.MouseRightButtonUp += new MouseButtonEventHandler(MapPane_MouseRightButtonUp);
			this.MouseMove += new MouseEventHandler(MapPane_MouseMove);
			this.MouseLeave += new MouseEventHandler(MapPane_MouseLeave);
			this.MouseEnter += new MouseEventHandler(MapPane_MouseEnter);
			this.MouseLeftButtonDown += new MouseButtonEventHandler(MapPane_MouseLeftButtonDown);
			this.MouseLeftButtonUp += new MouseButtonEventHandler(MapPane_MouseLeftButtonUp);
		}

		private void MapPane_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_leftClick)
			{
				_leftClick = false;

				if (TileToPlace != null)
				{
					AddTile((TileCluster)TileToPlace.Clone(), e.GetPosition(this), true);
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

			if (_hoverTile && TileToPlace != null)
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

		public void AddTile(TileCluster tile, Point where, bool relativeToCanvas)
		{
			tile.Position = relativeToCanvas ? CanvasToReal(where) : where;
			_tiles.Insert(tile);

			this.InvalidateVisual();
		}

		private Point CanvasToReal(Point canvasPoint)
		{
			canvasPoint.Offset(_origin - _offsetX, _origin - _offsetY);
			return canvasPoint;
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			double w = this.RenderSize.Width, h = this.RenderSize.Height;

			for (double i = 0.5; i <= (w + TileWidth); i += TileWidth)
			{
				double x = (i + (_offsetX - _origin) % TileWidth);
				dc.DrawLine(_gridPen, new Point(x, 0), new Point(x, h));
			}

			for (double i = 0.5; i <= (h + TileHeight); i += TileHeight)
			{
				double y = (i + (_offsetY - _origin) % TileHeight);
				dc.DrawLine(_gridPen, new Point(0, y), new Point(w, y));
			}

			foreach (TileCluster tc in _tiles.Query(new Rect(_origin - _offsetX, _origin - _offsetY, w, h)))
			{
				Vector offset = new Vector(_origin - _offsetX, _origin - _offsetY);

				tc.Draw(dc, offset);
			}

			if (_hoverTile)
			{
				if (TileToPlace != null)
				{
					TileToPlace.Draw(dc, _mouseHover, 0.5);
				}
			}
		}
	}
}
