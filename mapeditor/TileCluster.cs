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

		private Rect _bounds;
		private Size _drawSize, _tileSize;
		private Vector _borderOffset;
		private TileFactory _factory;
		private Tile _tile;

		public TileCluster(TileFactory tf, Size tileSize)
		{
			_factory = tf;
			_tile = tf.CreateTile();
//			_tile.Rotation = (TileRotation)90;
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
			// TODO: compensate for flip/rotate here
			if (corrected)
			{
				side += ((int)_tile.Rotation / 90);
				side %= 4;
			}

			switch (side)
			{
				case LEFTBORDER: return _factory.Borders.Left;
				case TOPBORDER: return _factory.Borders.Top;
				case RIGHTBORDER: return _factory.Borders.Right;
				case BOTTOMBORDER: return _factory.Borders.Bottom;
				default: return 0;
			}
		}

		private void UpdateDrawSize()
		{
			Size newSize = new Size();

			double imgWidth = _factory.Image.PixelWidth - GetBorder(LEFTBORDER, false) - GetBorder(RIGHTBORDER, false);
			double imgHeight = _factory.Image.PixelHeight - GetBorder(TOPBORDER, false) - GetBorder(BOTTOMBORDER, false);
			double width = _factory.TilesHorizontal * _tileSize.Width;
			double height = _factory.TilesVertical * _tileSize.Height;
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
			Point realPos = new Point(_tile.Location.X - _borderOffset.X, _tile.Location.Y - _borderOffset.Y);
			_bounds = new Rect(realPos, _drawSize);

			if (BoundsChanged != null)
				BoundsChanged(this, new EventArgs());
		}

		public void Draw(DrawingContext dc, Vector offset)
		{
			Point where = new Point(_bounds.X - offset.X, _bounds.Y - offset.Y);

			double center = Math.Min(_drawSize.Width / 2, _drawSize.Height / 2);
			dc.PushTransform(new RotateTransform((int)this._tile.Rotation, where.X + center, where.Y + center));
			dc.DrawImage(_factory.Image, new Rect(where, _drawSize));
			dc.Pop();
		}

		public void Draw(DrawingContext dc, Point where, double opacity)
		{
			where.Offset(-_borderOffset.X, -_borderOffset.Y);

			double center = Math.Min(_drawSize.Width / 2, _drawSize.Height / 2);
			dc.PushTransform(new RotateTransform((int)this._tile.Rotation, where.X + center, where.Y + center));
			dc.PushOpacity(opacity);
			dc.DrawImage(_factory.Image, new Rect(where, _drawSize));
			dc.Pop();
			dc.Pop();
		}
	}
}
