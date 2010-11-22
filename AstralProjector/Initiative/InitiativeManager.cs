using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

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
     * 
     */

    public class InitiativeManager
    {
        #region Static 
        private static DateTime _now = DateTime.Now;
        public static DateTime Now { get { return _now; } set { _now = value; } }

        internal const int FUTURE_TURNS = 5;
        internal const double FULLROUND_SECONDS = 6;
        internal const double STANDARD_ACTION_SECONDS = 4;
        internal const double MINOR_ACTION_SECONDS = 2.5;
        internal const double SWIFT_ACTION_SECONDS = 0.25;
        public static readonly TimeSpan FullRound = TimeSpan.FromSeconds(FULLROUND_SECONDS);
        public static readonly TimeSpan StandardAction = TimeSpan.FromSeconds(STANDARD_ACTION_SECONDS);
        public static readonly TimeSpan MinorAction = TimeSpan.FromSeconds(MINOR_ACTION_SECONDS);
        public static readonly TimeSpan SwiftAction = TimeSpan.FromSeconds(SWIFT_ACTION_SECONDS);

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

        private List<Event> _actors = new List<Event>();


        public InitiativeManager()
        {
        }

        public IList<Event> Events
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddActor(Actor actor)
        {
            throw new NotImplementedException();
        }

        public void AddEffect(SpellEffect bullsStrength)
        {
            throw new NotImplementedException();
        }

        // Reset for combat
        public void Shuffle()
        {
            throw new NotImplementedException();
        }

        public void MoveAfter(Event toMove, Event destination)
        {
            throw new NotImplementedException();
        }

        public void MoveBefore(Event toMove, Event destination)
        {
            throw new NotImplementedException();
        }

        public Event this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Event Next
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Clear(Team team)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Event e)
        {
            throw new NotImplementedException();
        }
    }
}
