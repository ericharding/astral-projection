using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Astral.Plane;
using Microsoft.Win32;

namespace Astral.Projector
{
    /// <summary>
    /// Interaction logic for DMScreen.xaml
    /// </summary>
    public partial class DMScreen : Window
    {
        private IMapDisplay _map;
        private PlayerViewController _pvc;

        public DMScreen()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(DMScreen_Loaded);
            _map = (IMapDisplay)_dmMapView;
            
        }

        void _map_MapPositionChanged(long x, long y)
        {
            _pvc.UpdateMapPosition(x, y);
        }

        void DMScreen_Loaded(object sender, RoutedEventArgs e)
        {
            _pvc = new PlayerViewController();
            _settingsPanel.DataContext = _pvc;

            _map.MapPositionChanged += _map_MapPositionChanged;
            _fog.FogChanged += new Action<double, double, int, bool>(_fog_FogChanged);

            foreach (var ex in _expanderCollection.Children.OfType<Expander>())
            {
                if (ex != null)
                {
                    ex.Expanded += new RoutedEventHandler(expander_Expanded);
                }
            }
        }

        void _fog_FogChanged(double x, double y, int size, bool clear)
        {
            _pvc.UpdateFogAt(x, y, size, clear);
        }

        void expander_Expanded(object sender, RoutedEventArgs e)
        {
            foreach (var ex in _expanderCollection.Children.OfType<Expander>())
            {
                if (ex != sender)
                    ex.IsExpanded = false;
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            _dmMapView.IsEnabled = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Astral Maps|*.astral";
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == true)
            {
                Map map = Map.LoadFromFile(ofd.FileName);
                _map.SetMap(map);
                _map.LayerMap[0] = true;
                _map.LayerMap[1] = true;
                for (int x = 2; x < map.Layers; x++)
                {
                    _map.LayerMap[x] = false;
                }
                _tbMapNotes.Text = map.Notes;

                _pvc.LoadMap(ofd.FileName);

                Dispatcher.In(TimeSpan.FromSeconds(0.2), () => _dmMapView.IsEnabled = true);
            }
        }

        private void _dmMapView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _dmMapView.TileSize = Math.Min(100, _dmMapView.TileSize + 5);
            }
            else
            {
                _dmMapView.TileSize -= Math.Min(5, _dmMapView.TileSize - 5);
            }
        }

        private void Layer_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb == null) return;

            bool isPlayer = cb.Content.ToString()[0] == 'P';
            int layer = Grid.GetRow(cb);

            if (isPlayer)
            {
                _pvc.SetLayerVisibility(layer, (bool)cb.IsChecked);
            }
            else
            {
                _map.LayerMap[layer] = (bool)cb.IsChecked;
            }
        }

        private void ResetFog_Button_Click(object sender, RoutedEventArgs e)
        {
            _pvc.ResetFog();
        }

        private void ManualAdjust_Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            string tag = fe.Tag.ToString();
            bool horizontal = tag == "left" || tag == "right";
            int offset = 1;
            if (tag == "left" || tag == "up")
                offset *= -1;

            _pvc.ManualAdjust(horizontal, offset);
        }

        private void _bShowImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.PNG;*.JPG;*.JPEG;*.GIF|All Files|*.*";
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == true)
            {
                string filename = ofd.FileName;
                _pvc.ShowImage(filename);
                _bHideImage.IsEnabled = true;
            }
        }

        private void _bHideImage_Click(object sender, RoutedEventArgs e)
        {
            _bHideImage.IsEnabled = false;
            _pvc.HideImage();
        }

        private void Effect_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            _pvc.DisplayEffect(fe.Tag.ToString());
        }

        private void GridConfig_Checked(object sender, RoutedEventArgs e)
        {
            if (_pvc == null) return;

            _dmMapView.IsDrawGridOver = false;
            _dmMapView.IsDrawGridUnder = false;

            if (_rbNoGrid.IsChecked == true)
            {
                _pvc.SetGridMode(0);
            }
            if (_rbUnderGrid.IsChecked == true)
            {
                _pvc.SetGridMode(1);
                _dmMapView.IsDrawGridUnder = true;
            }
            if (_rbOverGrid.IsChecked == true)
            {
                _pvc.SetGridMode(2);
                _dmMapView.IsDrawGridOver = true;
            }
        }
    }
}
