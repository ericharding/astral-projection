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
        public int TilesHoriz { get { return ArbitraryScale ? 1 : (int)tbTileHoriz.Value; } }
        public int TilesVert { get { return ArbitraryScale ? 1 : (int)tbTileVert.Value; } }
        public double BorderTop { get { return ArbitraryScale ? 0 : (int)tbBorderTop.Value; } }
        public double BorderRight { get { return ArbitraryScale ? 0 : (int)tbBorderRight.Value; } }
        public double BorderBottom { get { return ArbitraryScale ? 0 : (int)tbBorderBottom.Value; } }
        public double BorderLeft { get { return ArbitraryScale ? 0 : (int)tbBorderLeft.Value; } }
        public bool ArbitraryScale { get { return cbArbitrary.IsChecked == true; } }

        private bool _waitForIt = true;

        public TileImportDialog(string name, BitmapImage image)
        {
            InitializeComponent();

            imageTile.Source = image;
            imageTile.Width = image.PixelWidth;
            imageTile.Height = image.PixelHeight;

            tbTileName.Text = name.Split('.')[0];

            tbPixelsHoriz.Text = string.Format("{0}px", image.PixelWidth);
            tbPixelsVert.Text = string.Format("{0}px", image.PixelHeight);

            overlayTile.ImageSize = new Size(image.PixelWidth, image.PixelHeight);
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

        private void ToggleScaleControls(bool show)
        {
            UIElement[] hide = { nudHostBorderBottom, nudHostBorderLeft, nudHostBorderRight, nudHostBorderTop, rectBottom, rectRight, overlayTile };
            UIElement[] collapse = { nudHostTileHoriz, nudHostTileVert };

            foreach (UIElement h in hide)
                h.Visibility = show ? Visibility.Visible : Visibility.Hidden;

            foreach (UIElement c in collapse)
                c.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
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

        private void cbArbitrary_Checked(object sender, RoutedEventArgs e)
        {
            ToggleScaleControls(cbArbitrary.IsChecked != true);
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
        public Size ImageSize { get; set; }

        private Pen _gridPen;

        public OverlayGrid()
        {
            this.Background = Brushes.Transparent;
            _gridPen = new Pen(Brushes.OrangeRed, 1.5);
            _gridPen.DashStyle = DashStyles.Dash;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = this.RenderSize.Width, h = this.RenderSize.Height;
            double wi = this.ImageSize.Width, hi = this.ImageSize.Height;

            for (int i = 0; i <= TilesHoriz; i++)
            {
                double x = (i * ((wi - BorderRight - BorderLeft) / TilesHoriz)) + BorderLeft;
                x *= w / wi;
                dc.DrawLine(_gridPen, new Point(x, 0), new Point(x, h));
            }

            for (int i = 0; i <= TilesVert; i++)
            {
                double y = (i * ((hi - BorderBottom - BorderTop) / TilesVert)) + BorderTop;
                y *= h / hi;
                dc.DrawLine(_gridPen, new Point(0, y), new Point(w, y));
            }
        }
    }
}
