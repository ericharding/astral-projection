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

        public InitiativeManager InitiativeManager { get { return _unitInitiative; } }

        void InitiativeTracker_Loaded(object sender, RoutedEventArgs e)
        {
            _unitInitiative.EventsUpdated += new Action(_unitInitiative_EventsUpdated);
        }

        void _unitInitiative_EventsUpdated()
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
        Vector _mouseOffset;
        SimpleDragDropAdorner _dragAdorner;

        // Drag part

        private void _initiativeList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _sourceItemContainer = (ListBoxItem)_initiativeList.ContainerFromElement((DependencyObject)e.OriginalSource);
            if (_sourceItemContainer != null)
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
            if (_dragData != null && IsSufficientDelta(_initialMousePosition, e.GetPosition(_topWindow)))
            {
                DataObject data = new DataObject(DataFormats.GetDataFormat("DragDropItemsControl").Name, _dragData);
                //Point[] p = { _initialMousePosition, this.TranslatePoint(new Point(0, 0), _topWindow), _sourceItemContainer.TranslatePoint(new Point(0, 0), _topWindow) };
                //_mouseOffset = p[0] - new Point(p[1].X + p[2].X, p[1].Y + p[2].Y);
                _mouseOffset = _initialMousePosition - this.TranslatePoint(new Point(0, 0), _topWindow);

                //_mouseOffset +  .TranslatePoint(new Point(0, 0), _topWindow);

                Console.WriteLine("" + _initialMousePosition.X + ", " + _initialMousePosition.Y);
                Console.WriteLine("" + _mouseOffset.X + ", " + _mouseOffset.Y);

                _topWindow.AllowDrop = true;
                _topWindow.DragEnter += _topWindow_DragEnter;
                _topWindow.DragOver += _topWindow_DragOver;
                _topWindow.DragLeave += _topWindow_DragLeave;

                Console.WriteLine("Dragstart");

                DragDropEffects effect = DragDrop.DoDragDrop(this, _dragData, DragDropEffects.Move);

                Console.WriteLine("Dragstop");

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

        private SimpleDragDropAdorner CreateAdorner(ListBoxItem _sourceItemContainer)
        {
            Console.WriteLine("Create drag adorner");
            return new SimpleDragDropAdorner(_sourceItemContainer, this);
        }

        private void RemoveDragAdorner()
        {
            if (_dragAdorner != null)
            {
                Console.WriteLine("Remove drag adorner");
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
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        void _topWindow_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        // Drop part
        #region Drop
        private void _initiativeList_PreviewDragEnter(object sender, DragEventArgs e)
        {

        }

        private void _initiativeList_PreviewDragLeave(object sender, DragEventArgs e)
        {

        }

        private void _initiativeList_PreviewDragOver(object sender, DragEventArgs e)
        {

        }
        #endregion

        // Utility
        private bool IsSufficientDelta(Point origin, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - origin.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(currentPosition.Y - origin.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }

        #endregion


    }

    public class SimpleDragDropAdorner : Adorner
    {
        VisualBrush _brush;
        AdornerLayer _adornerLayer;
        UIElement _adornedElement;
        Rect _bounds;
        double _x, _y;

        public SimpleDragDropAdorner(FrameworkElement source, UIElement adornedElement)
            :base (adornedElement)
        {
            _brush = new VisualBrush(source);
            RenderOptions.SetCachingHint(_brush, CachingHint.Cache);
            this.Width = source.ActualWidth;
            this.Height = source.ActualHeight;
            _bounds = new Rect(0, 0, source.ActualWidth, source.ActualHeight);
            _adornedElement = adornedElement;
            _adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

            _adornerLayer.Add(this);
        }

        public void SetPosition(double x, double y)
        {
            Console.WriteLine("UpdatePos " + x + ", " + y);
            _x = x;
            _y = y;
            _adornerLayer.Update(_adornedElement);
        }

        public void Detach()
        {
            _adornerLayer.Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.PushOpacity(0.3);
            drawingContext.DrawRoundedRectangle(Brushes.GhostWhite, null, _bounds, 5, 5);
            drawingContext.Pop();
            drawingContext.PushOpacity(0.7);
            drawingContext.DrawRectangle(_brush, null, _bounds);

            Console.WriteLine("OnRender");
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_x, _y));
            return result;
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
