using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CSharpQuadTree;

namespace TileMap
{
	public class TileCluster : UIElement, IQuadObject, ICloneable
	{
		public string Text { get { return string.Format("{0} ({1}x{2})", _name, _tilesX, _tilesY); } }
		public BitmapImage Image { get { return _image; } }
		public event EventHandler BoundsChanged;
		public Rect Bounds { get { return _bounds; } }
		public Size TileSize { set { UpdateDrawSize(value); UpdateBounds(); } }
		public Point Position { set { _pos = value; UpdateBounds(); } }

		private string _name;
		private BitmapImage _image;
		private int _tilesX, _tilesY;
		private double _borderTop, _borderRight, _borderBottom, _borderLeft;
		private Point _pos;
		private Rect _bounds;
		private Size _drawSize;
		private Vector _borderOffset;

		public TileCluster(string name, BitmapImage image, int tilesX, int tilesY, double borderTop, double borderRight, double borderBottom, double borderLeft, Size tileSize)
		{
			_name = name;
			_image = image;
			_tilesX = tilesX;
			_tilesY = tilesY;
			_borderTop = borderTop;
			_borderRight = borderRight;
			_borderBottom = borderBottom;
			_borderLeft = borderLeft;
			TileSize = tileSize;
		}

		private const int LEFTBORDER = 1, TOPBORDER = 2, RIGHTBORDER = 3, BOTTOMBORDER = 4;
		private double GetBorder(int side)
		{
			// TODO: compensate for flip/rotate here

			switch (side)
			{
				case LEFTBORDER: return _borderLeft;
				case TOPBORDER: return _borderTop;
				case RIGHTBORDER: return _borderRight;
				case BOTTOMBORDER: return _borderBottom;
				default: return 0;
			}
		}

		private void UpdateDrawSize(Size tileSize)
		{
			Size newSize = new Size();

			double width = _tilesX * tileSize.Width;
			double height = _tilesY * tileSize.Height;
			double offsetL = (width * GetBorder(LEFTBORDER)) / _image.PixelWidth;
			double offsetR = (width * GetBorder(RIGHTBORDER)) / _image.PixelWidth;
			double offsetT = (height * GetBorder(TOPBORDER)) / _image.PixelHeight;
			double offsetB = (height * GetBorder(BOTTOMBORDER)) / _image.PixelHeight;
			newSize.Width = width + offsetL + offsetR;
			newSize.Height = height + offsetT + offsetB;

			_drawSize = newSize;

			_borderOffset = new Vector(offsetL, offsetT);
		}

		private void UpdateBounds()
		{
			Point realPos = new Point(_pos.X - _borderOffset.X, _pos.Y - _borderOffset.Y);
			_bounds = new Rect(realPos, _drawSize);

			if (BoundsChanged != null)
				BoundsChanged(this, new EventArgs());
		}

		public void Draw(DrawingContext dc, Vector offset)
		{
			dc.DrawImage(_image, new Rect(new Point(_bounds.X - offset.X, _bounds.Y - offset.Y), _drawSize));
		}

		public void Draw(DrawingContext dc, Point where, double opacity)
		{
			where.Offset(-_borderOffset.X, -_borderOffset.Y);
			dc.PushOpacity(opacity);
			dc.DrawImage(_image, new Rect(where, _drawSize));
			dc.Pop();
		}

		public object Clone()
		{
			return (TileCluster) this.MemberwiseClone();
		}
	}
}
