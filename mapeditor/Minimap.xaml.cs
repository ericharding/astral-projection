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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Astral.Plane;

namespace TileMap
{
    /// <summary>
    /// Interaction logic for Minimap.xaml
    /// </summary>
    public partial class Minimap : UserControl
    {
        public MapPane Map
        {
            set
            {
                if (_map != null)
                    _map.BitmapChanged -= Update;

                _map = value;

                if (_map != null)
                    _map.BitmapChanged += Update;

                Update();
            }
        }

        private MapPane _map;
        private VisualBrush _brush;

        public Minimap()
        {
            InitializeComponent();

            _brush = new VisualBrush();
            _brush.Stretch = Stretch.Uniform;
            canvasMap.Background = _brush;
        }

        private void Update()
        {
            if (_map != null)
                _brush.Visual = _map.GetEntireMapAsVisual();
        }
    }
}
