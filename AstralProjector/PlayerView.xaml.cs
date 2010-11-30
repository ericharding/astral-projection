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
using System.Diagnostics;
using Astral.Plane;
using TileMap;

namespace Astral.Projector
{
    /// <summary>
    /// Interaction logic for PlayerView.xaml
    /// </summary>
    public partial class PlayerView : Window
    {
        public PlayerView()
        {
            InitializeComponent();
        }

        public MapPane MapView { get { return _mapView; } }
        public FogOfWar Fog { get { return _fog; } }

    }
}
