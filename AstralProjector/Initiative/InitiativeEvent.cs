using System;
using System.Collections.Generic;
using System.Diagnostics;
using Astral.Plane.Utility;

namespace Astral.Projector.Initiative
{
    public enum Team
    {
        None,
        Blue,
        Gold,
        Green,
        Purple,
        Red,
        RedGold,
        Silver,
        BlueFlag,
        GreenFlag,
        PurpleFlag,
        RedFlag,
        YellowFlag
    }

    public enum ActionType
    {
        FullRound,
        Standard,
        Minor,
        Swift,
    }

    public abstract class Event : IComparable<Event>
    {
        protected string _name;
        protected InitiativeManager _manager;

        public Event(string name, InitiativeManager manager)
        {
            _name = name;
            _manager = manager;
            _scheduledAction = InitiativeManager.Now.Add(TimeSpan.FromTicks(RandomEx.Instance.Next(100000)));
        }

        #region properties

        public string Name { get { return _name; } }

        DateTime _scheduledAction;
        public DateTime ScheduledAction
        {
            get { return _scheduledAction; }
            protected set
            {
                DateTime oldTime = _scheduledAction;
                _scheduledAction = value;
                _manager.NotifyUpdated(this, oldTime);
            }
        }
        public IDictionary<string, string> Properties { get; set; }

        #endregion

        public void TakeAction(ActionType type)
        {
            Debug.Assert(ScheduledAction >= InitiativeManager.Now);

            ScheduledAction += InitiativeManager.GetActionLength(type);
        }

        public virtual void Complete()
        {
            this.TakeAction(ActionType.FullRound);
        }

        public void MoveAfter(Event other)
        {
            TimeSpan nextDelta = _manager.GetEventAfter(other).ScheduledAction - other.ScheduledAction;
            nextDelta = nextDelta.DivideBy(2);
            TimeSpan fixedNext = TimeSpan.FromMilliseconds(InitiativeManager.REACTION_TIME_MS);

            this.ScheduledAction = other.ScheduledAction + TimeSpanEx.Min(nextDelta, fixedNext);
        }

        public void MoveBefore(Event other)
        {
            Event before = _manager.GetEventBefore(other);
            if (before == null)
            {
                // play leap frog
                this.MoveAfter(other);
                other.MoveAfter(this);
            }
            else
            {
                TimeSpan delta = other.ScheduledAction - before.ScheduledAction;
                delta.DivideBy(2);
                TimeSpan fixedDelta = TimeSpan.FromMilliseconds(InitiativeManager.REACTION_TIME_MS);
                this.ScheduledAction = other.ScheduledAction - TimeSpanEx.Min(delta, fixedDelta);
            }
        }

        public override string ToString()
        {
            TimeSpan timeUntilMyTurn = ScheduledAction - InitiativeManager.Now;
            return string.Format("{0} (+{1}s)", this.Name, Math.Round(timeUntilMyTurn.TotalSeconds, 2));
        }

        internal void SetName(string newName)
        {
            _name = newName;
        }

        internal void SetNext(DateTime time)
        {
            this._scheduledAction = time;
        }

        #region IComparable

        public int CompareTo(Event other)
        {
            if (object.ReferenceEquals(this, other)) return 0;
            int result = this.ScheduledAction.CompareTo(other.ScheduledAction);
            if (result == 0)
            {
                if (Debugger.IsAttached) Debugger.Break();
                result = this.Name.CompareTo(other.Name);
            }
            return result;
        }

        public int CompareTo(object obj)
        {
            Event other = obj as Event;
            if (other == null) return -1;

            return this.CompareTo(other);
        }
        #endregion
    }

    // Player or monster
    public class Actor : Event
    {
        public Actor(string name, Team team, int hp, InitiativeManager m)
            : base(name, m)
        {
            this.Team = team;
            this.MaxHealth = this.CurrentHealth = hp;
        }

        public Team Team { get; private set; }
        public bool IsCasting { get; set; }

        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }

        public override string ToString()
        {
            return base.ToString() + string.Format(" hp: {0}/{1}", CurrentHealth, MaxHealth);
        }
    }

    public class TurnEnding : Event
    {
        private int _turn;

        public TurnEnding(int turn, DateTime time, InitiativeManager m)
            : base(string.Empty, m)
        {
            _turn = turn;
            SetNext(time);
            UpdateName();
        }

        public override void Complete()
        {
            DateTime newTime = _manager.Advanceturn(this, out _turn);
            UpdateName();
            this.ScheduledAction = newTime;
        }

        public int Turn { get { return _turn; } }
        private void UpdateName() { _name = "Turn " + _turn; }
    }

    public class SpellEffect : Event
    {
        private TimeSpan _duration;

        public SpellEffect(string name, TimeSpan duration, InitiativeManager m)
            : base(name, m)
        {
            _duration = duration;
            this.ScheduledAction = InitiativeManager.Now + _duration;
        }

        public override void Complete()
        {
            _manager.Remove(this);
        }
    }

}
