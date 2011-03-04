using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Astral.Plane;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Windows.Media;
using Astral.Projector.Initiative;

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
            _pvc.UpdateMapPosition(_map.PixelsToTiles(x, y));
            UpdatePlayerMapBounds();
        }

        private void UpdatePlayerMapBounds()
        {
            Rect playerMapBounds = _pvc.PlayerMapBounds;
            Point start = _map.TilesToPixels(playerMapBounds.X, playerMapBounds.Y);

            _playerMapBounds.Width = playerMapBounds.Width * _map.TileSize;
            _playerMapBounds.Height = playerMapBounds.Height * _map.TileSize;

            Rect localViewport = _map.MapViewport;
            double xdelt = localViewport.X - start.X;
            double ydelt = localViewport.Y - start.Y;

            _playerMapOffset.X = xdelt;
            _playerMapOffset.Y = ydelt;
        }

        void DMScreen_Loaded(object sender, RoutedEventArgs e)
        {
            _pvc = new PlayerViewController();
            _settingsPanel.DataContext = _pvc;

            _map.MapPositionChanged += _map_MapPositionChanged;
            _fog.FogChanged += _fog_FogChanged;

            foreach (var ex in _expanderCollection.Children.OfType<Expander>())
            {
                if (ex != null)
                {
                    ex.Expanded += new RoutedEventHandler(expander_Expanded);
                }
            }

            ResourceDictionary effects = (ResourceDictionary)Application.LoadComponent(new Uri("Effects/Effects.xaml", UriKind.Relative));
            _lbEffects.ItemsSource = effects.Keys;

            _initiativeTracker.InitiativeManager.EventsUpdated += InitiativeManager_EventsUpdated;
            _initiativeTracker.VisibleToPlayersChanged += new Action<bool>(_initiativeTracker_VisibleToPlayersChanged);
        }

        void _initiativeTracker_VisibleToPlayersChanged(bool visible)
        {
            _pvc.SetInitiativeVisibility(visible);
        }

        void InitiativeManager_EventsUpdated(InitiativeManager sender)
        {
            _pvc.UpdateInitiative(_initiativeTracker.InitiativeManager.Events);
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
                _fdMapNotes.Document = MakeFlowDocument(map.Notes);

                _pvc.LoadMap(ofd.FileName);

                Dispatcher.In(TimeSpan.FromSeconds(0.2), () => _dmMapView.IsEnabled = true);
                UpdatePlayerMapBounds();
                _playerMapBounds.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private System.Windows.Documents.FlowDocument MakeFlowDocument(string notes)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontSize = 12;
            doc.ColumnWidth = 900;

            if (string.IsNullOrEmpty(notes))
                return doc;

            Paragraph p = new Paragraph();
            p.TextAlignment = TextAlignment.Left;

            foreach (string s in notes.Trim().Split('\n'))
            {
                p.Inlines.Add(s);
                bool isEmpty = string.IsNullOrEmpty(s);

                if (isEmpty || Char.IsNumber(s[0]))
                {
                    if (p.Inlines.Count != 0)
                    {
                        p.BreakPageBefore = !isEmpty;
                        doc.Blocks.Add(p);
                        p = new Paragraph();
                    }
                }
            }

            doc.Blocks.Add(p);

            return doc;
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
            if (tag == "right" || tag == "down")
                offset *= -1;

            _pvc.ManualAdjust(horizontal, offset);
            UpdatePlayerMapBounds();
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

        private void _lbEffects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string effect = (string)_lbEffects.SelectedItem;
            if (!string.IsNullOrEmpty(effect))
            {
                _pvc.DisplayEffect(effect);
            }
        }

        private void _bClearEffects_Click(object sender, RoutedEventArgs e)
        {
            _lbEffects.SelectedItem = null;
            _pvc.ClearEffects();
        }
    }
}
