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
            _unitInitiative = new InitiativeManager(5,
                new UnitInitiative() { Name = "Joe", MaxHealth = 30, CurrentHealth = 30, Initiative = 10, TeamImage = "Images/gold.ico" },
                new UnitInitiative() { Name = "Mr. Stinky", MaxHealth = 30, CurrentHealth = 30, Initiative = 10, TeamImage = "/Images/green.ico" },
                new UnitInitiative() { Name = "Zombie 2", MaxHealth = 30, CurrentHealth = 30, Initiative = 10, TeamImage = "/Images/green.ico" });

            _initiativeList.ItemsSource = _unitInitiative;
            this.DataContext = _initiativeList;
        }


    }

}
