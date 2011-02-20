﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Diagnostics;

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

    public abstract class Event : IComparable
    {
        private string _name;

        public Event(string name)
        {
            _name = name;
            this.ScheduledAction = InitiativeManager.Now;
        }

        public string Name { get { return _name; } }

        public DateTime ScheduledAction { get; private set; }

        public void TakeAction(ActionType type)
        {
            Debug.Assert(ScheduledAction >= InitiativeManager.Now);
            if (ScheduledAction <= InitiativeManager.Now)
            {
                ScheduledAction = InitiativeManager.Now;
            }

            ScheduledAction += InitiativeManager.GetActionLength(type);
        }


        public int CompareTo(object obj)
        {
            Event other = obj as Event;
            if (other == null) return -1;

            return this.ScheduledAction.CompareTo(other.ScheduledAction);
        }

        public void MoveAfter(Event other)
        {
            throw new NotImplementedException();
        }

        public void MoveBefore(Event other)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    // Player or monster
    public class Actor : Event
    {
        public Actor(string name, Team team, int hp)
            : base(name)
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
        public TurnEnding(string name)
            : base(name)
        {

        }
    }

    public class SpellEffect : Event
    {
        private TimeSpan _duration;

        public SpellEffect(string name, TimeSpan duration)
            : base(name)
        {
            _duration = duration;
        }
    }

}
