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

namespace Astral.Projector
{
    /// <summary>
    /// Interaction logic for DMScreen.xaml
    /// </summary>
    public partial class DMScreen : Window
    {
        private IMapDisplay _map;

        public DMScreen()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(DMScreen_Loaded);
            _map = (IMapDisplay)this.FindName("_dmMapView");
            
        }

        void DMScreen_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var ex in _expanderCollection.Children.OfType<Expander>())
            {
                if (ex != null)
                {
                    ex.Expanded += new RoutedEventHandler(expander_Expanded);
                }
            }
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Astral Maps|*.astral";
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == true)
            {
                Map map = Map.LoadFromFile(ofd.FileName);
                _map.SetMap(map);

                var dims = _map.MapBounds;
            }
        }
    }
}
