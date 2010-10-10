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

            this.Loaded += Expander_Loaded;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {

        }

        private void Expander_Loaded(object sender, RoutedEventArgs e)
        {
            Expander self = gorramExpander;



            SearchChildren(self);
        }

        private static void SearchChildren(DependencyObject self)
        {
            for (int x = 0; x < VisualTreeHelper.GetChildrenCount(self); x++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(self, 0);
                var nameHaver = child as FrameworkElement;
                if (nameHaver != null)
                {
                    Console.WriteLine(nameHaver.Name);
                }

                SearchChildren(child);
            }
        }
    }
}
