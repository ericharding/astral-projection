using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Astral.Plane.Utility;

namespace Astral.Projector.Initiative
{
    /*
     * 1 Full turn is 6 seconds.
     * 
     * Actions:
     * Full action - 6 seconds
     * Standard action - 4 seconds
     * Move action - 2? seconds
     * Swift action - 0.5 seconds (or free but limited to 1 per 6 seconds?)
     * 
     * Actions page: http://www.d20srd.org/srd/combat/actionsincombat.htm#immediateActions
     * 
     */

    public class InitiativeManager
    {
        #region Static
        private static DateTime _now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
        public static DateTime Now { get { return _now; } set { _now = value; } }

        internal const double FULLROUND_SECONDS = 6;
        internal const double STANDARD_ACTION_SECONDS = 4;
        internal const double MINOR_ACTION_SECONDS = 2.5;
        internal const double SWIFT_ACTION_SECONDS = 0.25;
        internal const double REACTION_TIME_MS = 200;
        public static readonly TimeSpan FullRound = TimeSpan.FromSeconds(FULLROUND_SECONDS);
        public static readonly TimeSpan StandardAction = TimeSpan.FromSeconds(STANDARD_ACTION_SECONDS);
        public static readonly TimeSpan MinorAction = TimeSpan.FromSeconds(MINOR_ACTION_SECONDS);
        public static readonly TimeSpan SwiftAction = TimeSpan.FromSeconds(SWIFT_ACTION_SECONDS);

        internal const int TURNS_TO_SHOW = 8;

        internal static TimeSpan GetActionLength(ActionType type)
        {
            switch (type)
            {
                case ActionType.FullRound:
                    return FullRound;
                case ActionType.Standard:
                    return StandardAction;
                case ActionType.Minor:
                    return MinorAction;
                case ActionType.Swift:
                    return SwiftAction;
                default:
                    throw new ArgumentException("type");
            }
        }

        #endregion

        private List<Event> _events = new List<Event>();
        private Stack<Tuple<Event, DateTime>> _history = new Stack<Tuple<Event, DateTime>>();
        private EventFactory _eventFactory;
        private TurnEnding _lastRealizedTurn;

        public InitiativeManager()
        {
            _eventFactory = new EventFactory(this);
            CurrentTeam = Team.Blue;

            CreateTurns();
        }

        private void CreateTurns()
        {
            for (int x = 2; x <= TURNS_TO_SHOW + 1; x++)
            {
                DateTime time = Now.Add(TimeSpan.FromSeconds((x - 1) * FULLROUND_SECONDS));
                TurnEnding turn = new TurnEnding(x, time, this);
                _lastRealizedTurn = turn;
                _events.Add(turn);
            }
        }

        public event Action EventsUpdated = () => { };

        public IList<Event> Events
        {
            get
            {
                return _events.AsReadOnly();
            }
        }

        public Team CurrentTeam { get; set; }

        public void AddEvent(Event e)
        {
            // names ending in # auto increment
            HandleIncrementingName(e);

            int insertIndex = _events.BinarySearch(e);
            if (insertIndex < 0)
            {
                _events.Insert(~insertIndex, e);
            }
            else
            {
                throw new ArgumentException("event already exists");
            }

            EventsUpdated();
        }

        public void AddEvent(string desc)
        {
            AddEvent(CreateEvent(desc));
        }

        private void HandleIncrementingName(Event e)
        {
            if (e.Name.EndsWith("#"))
            {
                string baseName = e.Name.Substring(0, e.Name.Length - 1);
                int count = 1;
                while (_events.Find(o => o.Name == baseName + count) != null) count++;
                e.SetName(baseName + count);
            }
        }

        // spreads combatants out across the next 6 seconds
        public void Shuffle()
        {
            var actors = _events.OfType<Actor>().ToArray();
            actors = actors.OrderBy(a => RandomEx.Instance.NextDouble()).ToArray();
            double delta = ((FULLROUND_SECONDS) / actors.Length);
            for (int x = 0; x < actors.Length; x++)
            {
                actors[x].SetNext(InitiativeManager.Now.AddSeconds(delta * x).AddTicks(RandomEx.Instance.Next(100000)));
            }
            UpdateNow();
        }

        public void MoveAfter(Event toMove, Event destination)
        {
            toMove.MoveAfter(destination);
        }

        public void MoveBefore(Event toMove, Event destination)
        {
            toMove.MoveBefore(destination);
        }

        public Event this[string name]
        {
            get
            {
                return _events.Find(e => string.Compare(e.Name, name, true) == 0);
            }
        }

        public Event Next
        {
            get
            {
                return _events[0];
            }
        }

        public void Clear(Team team)
        {
            _events.RemoveAll(e => e is Actor && ((Actor)e).Team == team);
            UpdateNow();
        }

        public bool Remove(Event e)
        {
            bool result = _events.Remove(e);
            UpdateNow();
            return result;
        }

        private void UpdateNow()
        {
            _events.Sort();
            InitiativeManager.Now = _events.First().ScheduledAction;
        }

        // Clear spell effects and reset turn count
        public void Reset()
        {
            _events.RemoveAll(e => e is SpellEffect);
            _events.RemoveAll(e => e is TurnEnding);
            CreateTurns();
            _history.Clear();
            Shuffle();
        }

        public void Undo()
        {
            var top = _history.Pop();
            top.Item1.SetNext(top.Item2);
            UpdateNow();
        }

        public Event CreateEvent(string description)
        {
            return _eventFactory.Create(description);
        }

        public static int Roll(string dice)
        {
            return EventFactory.Roll(dice);
        }

        internal void NotifyUpdated(Event p, DateTime prevtime)
        {
            _history.Push(new Tuple<Event, DateTime>(p, prevtime));
            UpdateNow();
        }

        internal Event GetEventAfter(Event other)
        {
            int index = _events.BinarySearch(other);
            if (index + 1 < _events.Count)
            {
                return _events[index + 1];
            }
            else return null;
        }

        internal Event GetEventBefore(Event other)
        {
            int index = _events.BinarySearch(other);
            if (index == 0) return null;
            else return _events[index - 1];
        }

        internal DateTime Advanceturn(TurnEnding turnEnding, out int newturn)
        {
            Debug.Assert(turnEnding == _events[0]);
            DateTime newtime = _lastRealizedTurn.ScheduledAction.AddSeconds(FULLROUND_SECONDS);
            newturn = _lastRealizedTurn.Turn + 1;
            _lastRealizedTurn = turnEnding;
            return newtime;
        }
    }

    internal static class TimeSpanEx
    {
        public static TimeSpan Max(TimeSpan t1, TimeSpan t2)
        {
            return t1 > t2 ? t1 : t2;
        }

        public static DateTime Max(DateTime d1, DateTime d2)
        {
            return d1 > d2 ? d1 : d2;
        }

        public static TimeSpan Min(TimeSpan t1, TimeSpan t2)
        {
            return t1 > t2 ? t2 : t1;
        }

        public static DateTime Min(DateTime t1, DateTime t2)
        {
            return t1 > t2 ? t2 : t1;
        }

        public static TimeSpan DivideBy(this TimeSpan self, long divisor)
        {
            return TimeSpan.FromTicks(self.Ticks / divisor);
        }
    }

}
