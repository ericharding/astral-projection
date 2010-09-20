﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TileMap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ObservableCollection<TileCluster> _tiles = new ObservableCollection<TileCluster>();

		public MainWindow()
		{
			InitializeComponent();

			viewTiles.ItemsSource = _tiles;
		}

		private void bImport_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();

			if ((bool) open.ShowDialog())
			{
				Uri file = new Uri(open.FileName);
				BitmapImage img = new BitmapImage(file);
				TileImportDialog import = new TileImportDialog(open.SafeFileName, img);
				import.Owner = this;
				bool? result = import.ShowDialog();

				if ((bool) result)
				{
					TileCluster tc = new TileCluster(import.TileName, img, import.TilesHoriz, import.TilesVert, import.BorderTop, import.BorderRight, import.BorderBottom, import.BorderLeft, new Size(mapPane.TileWidth, mapPane.TileHeight));
					_tiles.Add(tc);
				}
			}
		}

		private void MenuItemDeleteImportedTile_Click(object sender, RoutedEventArgs e)
		{
			_tiles.Remove((TileCluster)(((MenuItem)e.Source).DataContext));
		}

		private void viewTiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			mapPane.TileToPlace = (TileCluster) viewTiles.SelectedItem;
		}

		private void viewTiles_MouseDown(object sender, MouseButtonEventArgs e)
		{
			viewTiles.SelectedItem = null;
		}
	}
}
