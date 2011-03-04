using System;
using System.Collections.Generic;
using System.Diagnostics;
using Astral.Plane.Utility;
using System.ComponentModel;

namespace Astral.Projector.Initiative
{
    public enum Team
    {
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

    [Serializable]
    public abstract class Event : IComparable<Event>, INotifyPropertyChanged
    {
        protected string _name;
        protected InitiativeManager _manager;

        public Event(string name, InitiativeManager manager)
        {
            _name = name;
            _manager = manager;
            _scheduledAction = InitiativeManager.Now.Add(TimeSpan.FromTicks(RandomEx.Instance.Next(100000)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
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

        public double SecondsUntilTurn
        {
            get
            {
                return Math.Round((ScheduledAction - InitiativeManager.Now).TotalSeconds, 2);
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
            Event nextEvent = _manager.GetEventAfter(other);
            if (nextEvent == null)
            {
                this.ScheduledAction = other.ScheduledAction.AddSeconds(InitiativeManager.REACTION_TIME_MS);
            }
            else
            {
                TimeSpan nextDelta = _manager.GetEventAfter(other).ScheduledAction - other.ScheduledAction;
                nextDelta = nextDelta.DivideBy(2);
                TimeSpan fixedNext = TimeSpan.FromMilliseconds(InitiativeManager.REACTION_TIME_MS);
                this.ScheduledAction = other.ScheduledAction + TimeSpanEx.Min(nextDelta, fixedNext);
            }
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
                delta = delta.DivideBy(2);
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

        internal virtual void TurnEnding(int turn)
        { }

        internal virtual void Activate()
        { }

        #region IComparable

        public int CompareTo(Event other)
        {
            if (object.ReferenceEquals(this, other)) return 0;
            int result = this.ScheduledAction.CompareTo(other.ScheduledAction);
            if (result == 0)
            {
                if (other is TurnEnding) return 1;
                if (this is TurnEnding) return -1;

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
    [Serializable]
    public class Actor : Event, INotifyPropertyChanged
    {
        public Actor(string name, Team team, int hp, InitiativeManager m)
            : base(name, m)
        {
            this.Team = team;
            this.MaxHealth = this.CurrentHealth = hp;
        }

        public Team Team { get; private set; }
        private bool _isCasting;
        public bool IsCasting { get { return _isCasting; } set { _isCasting = value; FirePropertyChanged("IsCasting"); } }
        private bool _isDead;
        public bool IsDead { get { return _isDead; } set { _isDead = value; FirePropertyChanged("IsDead"); } }

        private DateTime _nextAttackOfOpportunity = DateTime.MinValue;
        public bool HasAttackOfOpportunity
        {
            get
            {
                return _nextAttackOfOpportunity <= InitiativeManager.Now;
            }
            set
            {
                _nextAttackOfOpportunity = InitiativeManager.Now.AddSeconds(InitiativeManager.FULLROUND_SECONDS);
            }
        }

        public int MaxHealth { get; set; }
        private int _currentHealth;
        public int CurrentHealth
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                _currentHealth = value;
                FirePropertyChanged("CurrentHealth");
            }
        }

        internal override void Activate()
        { }

        public override string ToString()
        {
            return base.ToString() + string.Format(" hp: {0}/{1}", CurrentHealth, MaxHealth);
        }

    }

    [Serializable]
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
            if (this == _manager.Next)
            {
                _manager.Advanceturn(this);
            }
        }

        public int Turn
        {
            get
            { return _turn; }
            internal set
            {
                _turn = value;
                UpdateName();
            }
        }
        private void UpdateName() { _name = "Turn " + _turn; }
    }

    [Serializable]
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
