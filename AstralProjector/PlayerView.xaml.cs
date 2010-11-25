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

            this.Loaded += new RoutedEventHandler(PlayerView_Loaded);
        }

        void PlayerView_Loaded(object sender, RoutedEventArgs e)
        {
            Map map = Map.LoadFromFile(@"C:\Users\Eric\Documents\TestMap_export.astral");
            _dmMapView.SetMap(map);
        }

    }
}
