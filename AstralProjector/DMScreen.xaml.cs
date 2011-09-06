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
using System.IO;

namespace Astral.Projector
{
    enum View { DM, Player };

    /// <summary>
    /// Interaction logic for DMScreen.xaml
    /// </summary>
    public partial class DMScreen : Window
    {
        private IMapDisplay _map;
        private PlayerViewController _pvc;
        private AdventureTextFormatter _docFormatter;

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

            Action<string> navigate = (url) => { _webBrowser.Navigate(url); _expandD20SRD.IsExpanded = true; };
            _docFormatter = new AdventureTextFormatter(_initiativeTracker.InitiativeManager,
                new BasicAdventureLinkHandler("http", navigate),
                new BasicAdventureLinkHandler("map", (name) => MenuOpen_Click(null, null)),
                new AdventureLinkWithParens("link", navigate),
                new SpellLinkHandler("spell", navigate),
                new AdventureLinkWithParens("image", file => ShowImageEffect(file.Trim('"'))),
                new LayerLinkHandler("layer", ToggleLayerVisibility)
                );

            _docFormatter.FontFamily = _fdMapNotes.FontFamily;
            _docFormatter.FontSize = _fdMapNotes.FontSize;

            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1 && File.Exists(commandLineArgs[1]))
            {
                LoadMap(commandLineArgs[1]);
            }
        }

        void _initiativeTracker_VisibleToPlayersChanged(bool visible)
        {
            _pvc.SetInitiativeVisibility(visible);
        }

        void InitiativeManager_EventsUpdated(InitiativeManager sender)
        {
            _pvc.UpdateInitiative(_initiativeTracker.InitiativeManager.Events);
        }

        void _fog_FogChanged(double x, double y, int size, double alpha)
        {
            _pvc.UpdateFogAt(x, y, size, alpha);
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
                LoadMap(ofd.FileName);
            }
        }

        private void LoadMap(string fileName)
        {
            Map map = Map.LoadFromFile(fileName);
            _map.SetMap(map);
            _map.LayerMap[0] = true;
            _map.LayerMap[1] = true;
            for (int x = 2; x < map.Layers; x++)
            {
                _map.LayerMap[x] = false;
            }

            _fdMapNotes.Document = _docFormatter.MakeDocument(map.Notes);

            _pvc.LoadMap(fileName);

            Dispatcher.In(TimeSpan.FromSeconds(0.2), () => _dmMapView.IsEnabled = true);
            UpdatePlayerMapBounds();
            _playerMapBounds.Visibility = System.Windows.Visibility.Visible;
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

            View view = cb.Content.ToString()[0] == 'P' ? View.Player : View.DM;
            int layer = Grid.GetRow(cb);

            SetLayerVisibility(view, layer, (bool)cb.IsChecked);
        }

        private void ToggleLayerVisibility(View view, int layer)
        {
            if (view == View.Player)
            {
                _pvc.ToggleLayervisibility(layer);
            }
            else
            {
                _map.LayerMap[layer] = !_map.LayerMap[layer];
                _dmMapView.InvalidateVisual();
            }
        }

        private void SetLayerVisibility(View view, int layer, bool visible)
        {
            if (view == View.Player)
            {
                _pvc.SetLayerVisibility(layer, visible);
            }
            else
            {
                _map.LayerMap[layer] = visible;
                _dmMapView.InvalidateVisual();
            }
        }

        private void ResetFog_Button_Click(object sender, RoutedEventArgs e)
        {
            _fog.Reset();
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
                ShowImageEffect(filename);
            }
        }

        private void ShowImageEffect(string filename)
        {
            if (_bHideImage.IsEnabled)
            {
                HideImageEffect();
                return;
            }
            _pvc.ShowImage(filename);
            _bHideImage.IsEnabled = true;
        }

        private void _bHideImage_Click(object sender, RoutedEventArgs e)
        {
            HideImageEffect();
        }

        private void HideImageEffect()
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
