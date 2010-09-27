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
		public Size TileSize { set { UpdateDrawSize(value); UpdateBounds(); } }
		public Point Position { set { _tile.Location = value; UpdateBounds(); } }

		private Rect _bounds;
		private Size _drawSize;
		private Vector _borderOffset;
		private TileFactory _factory;
		private Tile _tile;

		public TileCluster(TileFactory tf, Size tileSize)
		{
			_factory = tf;
			_tile = tf.CreateTile();
			TileSize = tileSize;
		}

		private const int LEFTBORDER = 1, TOPBORDER = 2, RIGHTBORDER = 3, BOTTOMBORDER = 4;
		private double GetBorder(int side)
		{
			// TODO: compensate for flip/rotate here

			switch (side)
			{
				case LEFTBORDER: return _factory.Borders.Left;
				case TOPBORDER: return _factory.Borders.Top;
				case RIGHTBORDER: return _factory.Borders.Right;
				case BOTTOMBORDER: return _factory.Borders.Bottom;
				default: return 0;
			}
		}

		private void UpdateDrawSize(Size tileSize)
		{
			Size newSize = new Size();

			double width = _factory.TilesHorizontal * tileSize.Width;
			double height = _factory.TilesVertical * tileSize.Height;
			double offsetL = (width * GetBorder(LEFTBORDER)) / _factory.Image.PixelWidth;
			double offsetR = (width * GetBorder(RIGHTBORDER)) / _factory.Image.PixelWidth;
			double offsetT = (height * GetBorder(TOPBORDER)) / _factory.Image.PixelHeight;
			double offsetB = (height * GetBorder(BOTTOMBORDER)) / _factory.Image.PixelHeight;
			newSize.Width = width + offsetL + offsetR;
			newSize.Height = height + offsetT + offsetB;

			_drawSize = newSize;

			_borderOffset = new Vector(offsetL, offsetT);
		}

		private void UpdateBounds()
		{
			Point realPos = new Point(_tile.Location.X - _borderOffset.X, _tile.Location.Y - _borderOffset.Y);
			_bounds = new Rect(realPos, _drawSize);

			if (BoundsChanged != null)
				BoundsChanged(this, new EventArgs());
		}

		public void Draw(DrawingContext dc, Vector offset)
		{
			dc.DrawImage(_factory.Image, new Rect(new Point(_bounds.X - offset.X, _bounds.Y - offset.Y), _drawSize));
		}

		public void Draw(DrawingContext dc, Point where, double opacity)
		{
			where.Offset(-_borderOffset.X, -_borderOffset.Y);
			dc.PushOpacity(opacity);
			dc.DrawImage(_factory.Image, new Rect(where, _drawSize));
			dc.Pop();
		}
	}
}
