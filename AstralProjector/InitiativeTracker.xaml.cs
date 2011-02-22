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
using System.Diagnostics;

namespace Astral.Projector
{

    public partial class InitiativeTracker : UserControl
    {
        InitiativeManager _unitInitiative = new InitiativeManager();

        public InitiativeTracker()
        {
            InitializeComponent();


            foreach (var e in MockInitiativeData.testEvents)
            {
                _unitInitiative.AddEvent(e);
            }
            _initiativeList.ItemsSource = _unitInitiative.Events;
            this.Loaded += new RoutedEventHandler(InitiativeTracker_Loaded);
        }

        void InitiativeTracker_Loaded(object sender, RoutedEventArgs e)
        {
            _unitInitiative.EventsUpdated += new Action(_unitInitiative_EventsUpdated);
        }

        void _unitInitiative_EventsUpdated()
        {
            // perf cringe
            _initiativeList.Items.Refresh();
        }

        private void _initiativeList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void _initiativeList_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void ActionComplete(object sender, MouseButtonEventArgs e)
        {
            ((Event)_initiativeList.SelectedItem).Complete();

        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            _unitInitiative.Undo();
        }

        private void Heal_Click(object sender, RoutedEventArgs e)
        {
            foreach (var unit in _unitInitiative.Events.OfType<Actor>())
            {
                unit.CurrentHealth = unit.MaxHealth;
            }
            _initiativeList.Items.Refresh();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Team team = (Team)bottomTeam.SelectedIndex;
            _unitInitiative.Clear(team);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _unitInitiative.AddEvent(_tbAddText.Text);
            }
            catch
            {
                MessageBox.Show("Invalid.");
            }
        }

        private void _cbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _unitInitiative.CurrentTeam = (Team)_cbTeam.SelectedIndex;
        }

        private void FullAction_Click(object sender, RoutedEventArgs e)
        {
            TakeAction(sender, ActionType.FullRound);
        }

        private void StandardAction_Click(object sender, RoutedEventArgs e)
        {
            TakeAction(sender, ActionType.Standard);
        }

        private void MinorAction_Click(object sender, RoutedEventArgs e)
        {
            TakeAction(sender, ActionType.Minor);
        }

        private void QuickAction_Click(object sender, RoutedEventArgs e)
        {
            TakeAction(sender, ActionType.Swift);
        }

        private void TakeAction(object sender, ActionType actionType)
        {
            ListBoxItem container = (ListBoxItem)_initiativeList.ContainerFromElement((DependencyObject)sender);
            ((Event)container.Content).TakeAction(actionType);
        }
    }

    public class MockInitiativeData : IEnumerable<Event>
    {
        InitiativeManager _unitInitiative = new InitiativeManager();

        public static string[] testEvents = { "Joe The Magnificent Ranger hp:10 team:Gold", "Orc# hp:2d8 team:2", "Orc# hp:2d8 team:2", "Orc# hp:2d8 team:GreenFlag", "Bull's Strength dur:2" };

        public MockInitiativeData()
        {

            foreach (string te in testEvents)
            {
                _unitInitiative.AddEvent(te);
            }

            _unitInitiative["Orc1"].Complete();
            ((Actor)_unitInitiative["Orc1"]).HasAttackOfOpportunity = false;
            ((Actor)_unitInitiative["Orc2"]).IsDead = true;
            ((Actor)_unitInitiative["Orc3"]).IsCasting = true;
        }

        public IEnumerator<Event> GetEnumerator()
        {
            return _unitInitiative.Events.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

}
