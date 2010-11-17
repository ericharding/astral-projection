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
using Astral.Projector.Initiative;

namespace Astral.Projector
{
    /// <summary>
    /// Interaction logic for InitiativeTracker.xaml
    /// </summary>
    /// 


    /* todo
     * 
     * 1. Add Status effects - disabled / dying
     * 2. Add Health back on
     */
    public partial class InitiativeTracker : UserControl
    {
        InitiativeManager _unitInitiative;

        public InitiativeTracker()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(InitiativeTracker_Loaded);
        }

        void InitiativeTracker_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void _initiativeList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void _initiativeList_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }


    }

    

}
