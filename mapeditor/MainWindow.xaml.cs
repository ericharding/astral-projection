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
using Astral.Plane;

namespace TileMap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Map _library = new Map();

		public MainWindow()
		{
			InitializeComponent();

			viewTiles.ItemsSource = _library.TileFactories;
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
					TileFactory tf = new TileFactory(img, import.TileName, new Borders(import.BorderLeft, import.BorderTop, import.BorderRight, import.BorderBottom), import.TilesHoriz, import.TilesVert);
					_library.AddTileFactory(tf);
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
	}
}
