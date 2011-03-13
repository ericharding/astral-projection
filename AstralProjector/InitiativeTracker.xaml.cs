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
using Astral.Projector.Initiative.View;
using Astral.Plane.Controls;
using System.Windows.Controls.Primitives;

namespace Astral.Projector
{

    public partial class InitiativeTracker : UserControl
    {
        InitiativeManager _unitInitiative = new InitiativeManager();
        InitiativeBroadcaster _broadcaster;

        public InitiativeTracker()
        {
            InitializeComponent();

            _initiativeList.ItemsSource = _unitInitiative.Events;
            this.Loaded += new RoutedEventHandler(InitiativeTracker_Loaded);
            this._broadcaster = new InitiativeBroadcaster(_unitInitiative);
        }

        public InitiativeManager InitiativeManager { get { return _unitInitiative; } }

        private bool _visibleToPlayers;
        public event Action<bool> VisibleToPlayersChanged = (_) => { };
        public bool IsVisibleToPlayers
        {
            get { return _visibleToPlayers; }
            set
            {
                _visibleToPlayers = value;
                VisibleToPlayersChanged(value);
            }
        }

        void InitiativeTracker_Loaded(object sender, RoutedEventArgs e)
        {
            _unitInitiative.EventsUpdated += _unitInitiative_EventsUpdated;
        }

        void _unitInitiative_EventsUpdated(InitiativeManager unused)
        {
            // perf cringe
            _initiativeList.Items.Refresh();
        }

        private void ActionComplete(object sender, MouseButtonEventArgs e)
        {
            EventFromSender(sender).Complete();

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
            TakeAction(e.Source, ActionType.FullRound);
        }

        private void StandardAction_Click(object sender, RoutedEventArgs e)
        {
            TakeAction(e.Source, ActionType.Standard);
        }

        private void MinorAction_Click(object sender, RoutedEventArgs e)
        {
            TakeAction(e.Source, ActionType.Minor);
        }

        private void QuickAction_Click(object sender, RoutedEventArgs e)
        {
            TakeAction(e.Source, ActionType.Swift);
        }

        private void TakeAction(object sender, ActionType actionType)
        {
            EventFromSender(sender).TakeAction(actionType);
        }

        private Event EventFromSender(object sender)
        {
            ListBoxItem container = (ListBoxItem)_initiativeList.ContainerFromElement((DependencyObject)sender);
            if (container != null)
            {
                return ((Event)container.Content);
            }
            return null;
        }

        #region Drag/Drop

        Event _dragData;
        ListBoxItem _sourceItemContainer;
        Point _initialMousePosition;
        Window _topWindow;
        Point _mouseOffset;
        SimpleDragAdorner _dragAdorner;

        ListBoxItem _dropTargetItemContainer;
        Event _dropTargetItem;
        bool _isInFirstHalf;
        InsertionAdorner _insertionAdorner;

        // Drag part
        #region Drag
        private void _initiativeList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _sourceItemContainer = (ListBoxItem)_initiativeList.ContainerFromElement((DependencyObject)e.OriginalSource);
            if (_sourceItemContainer != null && !IsOverThumb(e))
            {
                _dragData = (Event)_sourceItemContainer.Content;

                _topWindow = Window.GetWindow(this);
                _initialMousePosition = e.GetPosition(_topWindow);
            }
        }

        private void _initiativeList_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragData = null;
            _sourceItemContainer = null;
        }

        private void _initiativeList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragData != null && IsSufficientDelta(_initialMousePosition, e.GetPosition(_topWindow)) && !IsOverThumb(e))
            {
                DataObject data = new DataObject(DataFormats.GetDataFormat("DragDropItemsControl").Name, _dragData);
                
                Point mouseWindowOffset = this.TranslatePoint(new Point(0, 0), _topWindow);
                Point mouseContainerOffset = e.GetPosition(_sourceItemContainer);

                _mouseOffset = new Point(_initialMousePosition.X - mouseWindowOffset.X - mouseContainerOffset.X,
                                         _initialMousePosition.Y - mouseWindowOffset.Y - mouseContainerOffset.Y);

                _topWindow.AllowDrop = true;
                _topWindow.DragEnter += _topWindow_DragEnter;
                _topWindow.DragOver += _topWindow_DragOver;
                _topWindow.DragLeave += _topWindow_DragLeave;

                // Console.WriteLine("Dragstart");
                DragDropEffects effect = DragDrop.DoDragDrop(this, _dragData, DragDropEffects.Move);
                // Console.WriteLine("Dragstop");

                RemoveDragAdorner();
                _topWindow.DragEnter -= _topWindow_DragEnter;
                _topWindow.DragOver -= _topWindow_DragOver;
                _topWindow.DragLeave -= _topWindow_DragLeave;
                _dragData = null;
            }
        }

        private void ShowDragAdorner(Point point)
        {
            if (_dragAdorner == null)
            {
                _dragAdorner = CreateAdorner(_sourceItemContainer);
            }
            _dragAdorner.SetPosition(
                point.X - _initialMousePosition.X + _mouseOffset.X,
                point.Y - _initialMousePosition.Y + _mouseOffset.Y);
        }

        private SimpleDragAdorner CreateAdorner(ListBoxItem _sourceItemContainer)
        {
            return new SimpleDragAdorner(_sourceItemContainer, this);
        }

        private void RemoveDragAdorner()
        {
            if (_dragAdorner != null)
            {
                _dragAdorner.Detach();
                _dragAdorner = null;
            }
        }

        void _topWindow_DragEnter(object sender, DragEventArgs e)
        {
            ShowDragAdorner(e.GetPosition(_topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        void _topWindow_DragOver(object sender, DragEventArgs e)
        {
            ShowDragAdorner(e.GetPosition(_topWindow));
            e.Handled = true;
        }

        void _topWindow_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        // Drop part
        #region Drop
        private void _initiativeList_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (_dragData != null)
            {
                DecideDropTarget(e);
                UpdateInsertionAdorner();
            }
            e.Effects = e.AllowedEffects;
            e.Handled = true;
        }

        private void _initiativeList_PreviewDragOver(object sender, DragEventArgs e)
        {
            DecideDropTarget(e);
            UpdateInsertionAdorner();

            e.Effects = DragDropEffects.Move;
        }

        private void _initiativeList_PreviewDragLeave(object sender, DragEventArgs e)
        {
            if (_dragData != null)
            {
                RemoveInsertionAdorner();
            }
        }

        private void _initiativeList_Drop(object sender, DragEventArgs e)
        {
            if (_dragData == null) return;
            if (_isInFirstHalf)
            {
                _dragData.MoveBefore(_dropTargetItem);
            }
            else
            {
                _dragData.MoveAfter(_dropTargetItem);
            }
        }

        private void DecideDropTarget(DragEventArgs e)
        {
            _dropTargetItemContainer = (ListBoxItem)_initiativeList.ContainerFromElement((DependencyObject)e.OriginalSource);
            if (_dropTargetItemContainer != null)
            {

                Point positionRelativeToContainer = e.GetPosition(_dropTargetItemContainer);
                _isInFirstHalf = positionRelativeToContainer.Y < (_dropTargetItemContainer.ActualHeight/2);

                _dropTargetItem = (Event)_dropTargetItemContainer.Content;
            }
            else
            {
                _dropTargetItem = null;
            }
        }

        private void UpdateInsertionAdorner()
        {
            if (_insertionAdorner == null && _dropTargetItemContainer != null)
            {
                _insertionAdorner = new InsertionAdorner(_isInFirstHalf, _dropTargetItemContainer);
            }
            if (_insertionAdorner != null)
            {
                _insertionAdorner.IsInFirstHalf = _isInFirstHalf;
            }
        }

        private void RemoveInsertionAdorner()
        {
            if (_insertionAdorner != null)
            {
                _insertionAdorner.Detach();
                _insertionAdorner = null;
            }
        }

        #endregion

        // Utility
        private bool IsSufficientDelta(Point origin, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - origin.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(currentPosition.Y - origin.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }

        private bool IsOverThumb(MouseEventArgs e)
        {
            var element = this.InputHitTest(e.GetPosition(this)) as DependencyObject;
            do
            {
                element = VisualTreeHelper.GetParent(element);
                if (element is Thumb)
                {
                    return true;
                }

            } while (element != null);

            return false;
        }

        #endregion

        private void _tbAddText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Add_Click(this, null);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.IsVisibleToPlayers = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.IsVisibleToPlayers = false;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _unitInitiative.Reset();
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (_dragData != null)
            {
                _unitInitiative.Remove(_dragData);
            }
        }
    }

    public class MockInitiativeData : IEnumerable<Event>
    {
        InitiativeManager _unitInitiative = new InitiativeManager();

        public static string[] testEvents = { "Joe The Magnificent Ranger of DOOM hp:10 team:Gold", "Orc# hp:2d8 team:2", "Orc# hp:2d8 team:2", "Orc# hp:2d8 team:GreenFlag", "Bull's Strength dur:2" };

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
