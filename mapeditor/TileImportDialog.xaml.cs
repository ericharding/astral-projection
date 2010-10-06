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
using System.Windows.Shapes;

namespace TileMap
{
	/// <summary>
	/// Interaction logic for TileImportDialog.xaml
	/// </summary>
	public partial class TileImportDialog : Window
	{
		public string TileName { get { return tbTileName.Text; } }
		public int TilesHoriz { get { return (int) tbTileHoriz.Value; } }
		public int TilesVert { get { return (int) tbTileVert.Value; } }
		public double BorderTop { get { return (int) tbBorderTop.Value; } }
		public double BorderRight { get { return (int) tbBorderRight.Value; } }
		public double BorderBottom { get { return (int) tbBorderBottom.Value; } }
		public double BorderLeft { get { return (int) tbBorderLeft.Value; } }

		private bool _waitForIt = true;

		public TileImportDialog(string name, BitmapImage image)
		{
			InitializeComponent();
			RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

			imageTile.Source = image;

			tbTileName.Text = name.Split('.')[0];

			tbPixelsHoriz.Text = string.Format("{0}px", image.PixelWidth);
			tbPixelsVert.Text = string.Format("{0}px", image.PixelHeight);
		}

		private void Update()
		{
			overlayTile.TilesHoriz = TilesHoriz;
			overlayTile.TilesVert = TilesVert;
			overlayTile.BorderTop = BorderTop;
			overlayTile.BorderRight = BorderRight;
			overlayTile.BorderBottom = BorderBottom;
			overlayTile.BorderLeft = BorderLeft;
			overlayTile.InvalidateVisual();
		}

		private void NumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (!_waitForIt)
				Update();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_waitForIt = false;
			Update();
		}

		private void bImport_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}

	public class OverlayGrid : Control
	{
		public int TilesHoriz { get; set; }
		public int TilesVert { get; set; }
		public double BorderTop { get; set; }
		public double BorderRight { get; set; }
		public double BorderBottom { get; set; }
		public double BorderLeft { get; set; }

		private Pen _gridPen;

		public OverlayGrid()
		{
			this.Background = Brushes.Transparent;
			_gridPen = new Pen(Brushes.OrangeRed, 2);
			_gridPen.DashStyle = DashStyles.Dash;
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			double w = this.RenderSize.Width, h = this.RenderSize.Height;

			for (int i = 0; i <= TilesHoriz; i++)
			{
				double x = (i * ((w - BorderRight - BorderLeft) / TilesHoriz)) + BorderLeft;
				dc.DrawLine(_gridPen, new Point(x, 0), new Point(x, h));
			}

			for (int i = 0; i <= TilesVert; i++)
			{
				double y = (i * ((h - BorderBottom - BorderTop) / TilesVert)) + BorderTop;
				dc.DrawLine(_gridPen, new Point(0, y), new Point(w, y));
			}
		}
	}
}
