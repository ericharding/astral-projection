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
using Microsoft.Win32;
using Astral.Plane;
using System.Windows.Threading;

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
                _map.LayerMap.SetAll(false);
                _map.LayerMap[0] = true;
                if (_map.LayerMap.Count > 1)
                    _map.LayerMap[1] = true;
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
                
                // hackhack
                _dmMapView.InvalidateVisual();
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
    }
}
