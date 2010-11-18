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
using System.Windows.Shapes;
using Microsoft.Win32;
using Astral.Plane;

namespace TileMap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Map _library = new Map();
		private readonly string _libraryFileName = AppDomain.CurrentDomain.BaseDirectory + "library.astral";
		private const string _fileFilter = "Astral Projection files (*.astral)|*.astral|All files (*.*)|*.*";

		public MainWindow()
		{
			InitializeComponent();

			mapPane.OnFileInfoUpdated += new Action(this.UpdateTitle);
			UpdateTitle();

			if (File.Exists(_libraryFileName))
				_library = Map.LoadFromFile(_libraryFileName);

			mapPane.SetLibrary(_library);

			viewTiles.ItemsSource = _library.TileFactories;
		}

		private void bImport_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();

			if ((bool) open.ShowDialog(this))
			{
				Uri file = new Uri(open.FileName);
				BitmapImage img = new BitmapImage(file);
				TileImportDialog import = new TileImportDialog(open.SafeFileName, img);
				import.Owner = this;
				bool? result = import.ShowDialog();

				if ((bool) result)
				{
					TileFactory tf = new TileFactory(img, import.TileName, new Borders(import.BorderLeft, import.BorderTop, import.BorderRight, import.BorderBottom), import.TilesHoriz, import.TilesVert);
					_library.AddTileFactory(tf);
                    _library.Save(_libraryFileName);
				}
			}
		}

		private void MenuItemDeleteImportedTile_Click(object sender, RoutedEventArgs e)
		{
			// TODO: confirm this action; delete instances of deleted tile from map?

			//_tiles.Remove((TileCluster)(((MenuItem)e.Source).DataContext));
			// TODO: Map needs a Remove()
		}

		private void viewTiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			mapPane.TileToPlace = (TileFactory) viewTiles.SelectedItem;
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

			switch (e.Key)
			{
				case Key.Escape:
					viewTiles.SelectedItem = null;
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
					save.Filter = _fileFilter;

					if ((bool)save.ShowDialog(this))
					{
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
			open.Filter = _fileFilter;

			if ((bool) open.ShowDialog(this))
			{
				mapPane.Clear();

				Map load = Map.LoadFromFile(open.FileName);

				// TODO: sanity check

				mapPane.SetMap(load);
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

		private void menuExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !SaveIfNeeded(false);
		}
	}
}
