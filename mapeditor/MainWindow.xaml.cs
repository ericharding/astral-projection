using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using Astral.Plane;
using Astral.Plane.Utility;

namespace TileMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<TileFactory> _filteredLibrary;
        private Map _library = new Map();
        private Settings _prefs = new Settings();
        // private readonly string _libraryFileName = AppDomain.CurrentDomain.BaseDirectory + "library.astral";
        private readonly string _libraryFileName = Environment.CurrentDirectory + "\\library.astral";
        private const string _fileFilter = "Astral Projection files (*.astral)|*.astral|All files (*.*)|*.*";

        public MainWindow()
        {
            InitializeComponent();

            mapPane.OnFileInfoUpdated += new Action(this.UpdateTitle);
            UpdateTitle();

            if (File.Exists(_libraryFileName))
                _library = Map.LoadFromFile(_libraryFileName);

            _prefs.Load();

            mapPane.SetLibrary(_library);

            _filteredLibrary = new ObservableCollection<TileFactory>(_library.TileFactories);
            viewTiles.ItemsSource = _filteredLibrary;
        }

        private void bImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = (string)_prefs["defaultDirImport"];

            if ((bool)open.ShowDialog(this))
            {
                _prefs["defaultDirImport"] = Path.GetDirectoryName(open.FileName);
                Uri file = new Uri(open.FileName);
                BitmapImage img = new BitmapImage(file);
                TileImportDialog import = new TileImportDialog(open.SafeFileName, img);
                import.Owner = this;
                bool? result = import.ShowDialog();

                if ((bool)result)
                {
                    TileFactory tf = new TileFactory(img, import.TileName, new Borders(import.BorderLeft, import.BorderTop, import.BorderRight, import.BorderBottom), import.TilesHoriz, import.TilesVert);
                    _library.AddTileFactory(tf);
                    SaveLibrary();
                }
            }
        }

        private void SaveLibrary()
        {
            if (File.Exists(_libraryFileName))
            {
                File.Delete(_libraryFileName + ".bak");
                File.Move(_libraryFileName, _libraryFileName + ".bak");
            }
            _library.Save(_libraryFileName);
            UpdateFilteredLibrary(tbSearchLibrary.Text);
        }

        private void MenuItemDeleteImportedTile_Click(object sender, RoutedEventArgs e)
        {
            // TODO: confirm this action; delete instances of deleted tile from map?

            TileFactory factory = viewTiles.SelectedItem as TileFactory;
            try
            {
                if (factory != null)
                {
                    _library.RemoveTileFactory(factory);
                    _filteredLibrary.Remove(factory);
                }
                SaveLibrary();
            }
            catch (InvalidOperationException io)
            {
                MessageBox.Show(io.Message);
            }
        }

        private void viewTiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mapPane.TileToPlace = (TileFactory)viewTiles.SelectedItem;
        }

        private void viewTiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewTiles.SelectedItem = null;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                switch (e.Key)
                {
                    case Key.S:
                        menuSaveAs_Click(null, null);
                        break;
                    case Key.E:
                        menuExport_Click(null, null);
                        break;
                }

                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        menuNew_Click(null, null);
                        break;
                    case Key.O:
                        menuOpen_Click(null, null);
                        break;
                    case Key.S:
                        menuSave_Click(null, null);
                        break;
                }

                return;
            }

            if (_mapNotes.IsFocused || tbSearchLibrary.IsFocused) return;

            switch (e.Key)
            {
                case Key.Escape:
                    viewTiles.SelectedItem = null;
                    viewTiles_SelectionChanged(null, null);
                    mapPane.InvalidateVisual();
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    mapPane.IsSnapToGrid = false;
                    break;
                case Key.R:
                    mapPane.RotatePreview(true);
                    break;
                case Key.L:
                    mapPane.RotatePreview(false);
                    break;
                case Key.H:
                    mapPane.MirrorPreview(true);
                    break;
                case Key.V:
                    mapPane.MirrorPreview(false);
                    break;
                case Key.D0:
                    menuGoToOrigin_Click(null, null);
                    break;
                case Key.Y:
                    mapPane.PickUpTile();
                    UpdateFilteredLibrary(tbSearchLibrary.Text);
                    break;
                case Key.NumPad0:
                    this._layer.SelectedIndex = 0;
                    break;
                case Key.NumPad1:
                    this._layer.SelectedIndex = 1;
                    break;
                case Key.NumPad2:
                    this._layer.SelectedIndex = 2;
                    break;
                case Key.NumPad3:
                    this._layer.SelectedIndex = 3;
                    break;
                case Key.NumPad4:
                    this._layer.SelectedIndex = 4;
                    break;
                case Key.NumPad5:
                    this._layer.SelectedIndex = 5;
                    break;
                case Key.NumPad6:
                    this._layer.SelectedIndex = 6;
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    mapPane.IsSnapToGrid = true;
                    break;
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Zoom(5);
            else if (e.Delta < 0)
                Zoom(-5);
        }

        private bool SaveIfNeeded(bool saveAs, bool warn = true)
        {
            if (saveAs || mapPane.Dirty)
            {
                if (warn)
                {
                    MessageBoxResult result = MessageBox.Show("The map has been changed since the last save.\n\nSave now?", "Save?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);

                    if (result == MessageBoxResult.Cancel)
                        return false;
                    else if (result == MessageBoxResult.No)
                        return true;
                }

                if (saveAs || string.IsNullOrEmpty(mapPane.FileName))
                {
                    SaveFileDialog save = new SaveFileDialog();
                    save.InitialDirectory = (string)_prefs["defaultDirSave"];
                    save.Filter = _fileFilter;

                    if ((bool)save.ShowDialog(this))
                    {
                        _prefs["defaultDirSave"] = Path.GetDirectoryName(save.FileName);
                        mapPane.Save(save.FileName);

                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    mapPane.Save();

                    return true;
                }
            }

            return true;
        }

        private void Zoom(int pixels)
        {
            if (mapPane.TileWidth + pixels <= 0 || mapPane.TileHeight + pixels <= 0)
                return;

            mapPane.TileWidth += pixels;
            mapPane.TileHeight += pixels;
        }

        private void UpdateTitle()
        {
            this.Title = "Astral Map - " + (mapPane.FileName ?? "(no file)") + (mapPane.Dirty ? "*" : "");
        }

        private void UpdateFilteredLibrary(string filter)
        {
            viewTiles.SelectionChanged -= viewTiles_SelectionChanged;

            _filteredLibrary.Clear();

            foreach (TileFactory tf in _library.TileFactories)
            {
                foreach (string tag in tf.Tags)
                {
                    if (tag.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase))
                    {
                        _filteredLibrary.Add(tf);

                        goto TryNextFactory;
                    }
                }
            TryNextFactory:
                continue;
            }

            if (_filteredLibrary.Contains(mapPane.TileToPlace))
                viewTiles.SelectedItem = mapPane.TileToPlace;

            viewTiles.SelectionChanged += new SelectionChangedEventHandler(viewTiles_SelectionChanged);
        }

        private void menuNew_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveIfNeeded(false))
                return;

            mapPane.Clear();
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveIfNeeded(false))
                return;

            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = (string)_prefs["defaultDirOpen"];
            open.Filter = _fileFilter;

            if ((bool)open.ShowDialog(this))
            {
                _prefs["defaultDirOpen"] = Path.GetDirectoryName(open.FileName);
                Map load = Map.LoadFromFile(open.FileName);
                load.AddReference(_library);

                // TODO: sanity check

                mapPane.SetMap(load);

                _mapNotes.Text = load.Notes;
            }
        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            this.SaveIfNeeded(false, false);
        }

        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            this.SaveIfNeeded(true, false);
        }

        private void menuExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = (string)_prefs["defaultDirExport"];
            save.Filter = _fileFilter;
            save.Title = "Export";

            if ((bool)save.ShowDialog(this))
            {
                _prefs["defaultDirExport"] = Path.GetDirectoryName(save.FileName);
                mapPane.Export(save.FileName);
            }
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void menuGoToOrigin_Click(object sender, RoutedEventArgs e)
        {
            mapPane.SetMapPosition(-1, -1);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !SaveIfNeeded(false);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _prefs.Save();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mapPane != null)
            {
                mapPane.MapNotes = _mapNotes.Text;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            int x = Int32.Parse(cb.Tag.ToString());
            if (x < mapPane.LayerMap.Count)
            {
                mapPane.LayerMap[x] = (bool)cb.IsChecked;
                mapPane.InvalidateVisual();
            }
        }

        private void tbSearchLibrary_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilteredLibrary(tbSearchLibrary.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                viewTiles.Focus();
                e.Handled = true;
            }
        }
    }
}
