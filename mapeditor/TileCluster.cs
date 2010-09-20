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
		public Size TileSize { set { _drawSize = new Size(_tilesX * value.Width, _tilesY * value.Height); UpdateBounds(); } }
		public Point Position { set { _pos = value; UpdateBounds(); } }

		private string _name;
		private BitmapImage _image;
		private int _tilesX, _tilesY;
		private double _borderTop, _borderRight, _borderBottom, _borderLeft;
		private Point _pos;
		private Rect _bounds;
		private Size _drawSize;

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

		private void UpdateBounds()
		{
			_bounds = new Rect(_pos, _drawSize);

			if (BoundsChanged != null)
				BoundsChanged(this, new EventArgs());
		}

		public void Draw(DrawingContext dc, Vector offset)
		{
			dc.DrawImage(_image, new Rect(new Point(_bounds.X - offset.X, _bounds.Y - offset.Y), _drawSize));
		}

		public void Draw(DrawingContext dc, Point where, double opacity)
		{
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
