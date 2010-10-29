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

    public class TurnManager :IEnumerable<InitiativeEvent>
    {
        #region Static 
        private static DateTime _now = DateTime.Now;
        public static DateTime Now { get { return _now; } set { _now = value; } }

        internal const int FUTURE_TURNS = 5;
        internal const double FULLROUND_SECONDS = 6;
        internal const double STANDARD_ACTION_SECONDS = 4;
        internal const double MINOR_ACTION_SECONDS = 2.5;
        internal const double SWIFT_ACTION_SECONDS = 0.25;
        internal static readonly TimeSpan FullRound = TimeSpan.FromSeconds(FULLROUND_SECONDS);
        internal static readonly TimeSpan StandardAction = TimeSpan.FromSeconds(STANDARD_ACTION_SECONDS);
        internal static readonly TimeSpan MinorAction = TimeSpan.FromSeconds(MINOR_ACTION_SECONDS);
        internal static readonly TimeSpan SwiftAction = TimeSpan.FromSeconds(SWIFT_ACTION_SECONDS);

        #endregion

        private List<InitiativeEvent> _actors = new List<InitiativeEvent>();


        public TurnManager()
        {
        }


        public void AddActor(string description)
        {
        }




        public IEnumerator<InitiativeEvent> GetEnumerator()
        {
            _actors.Sort();
            List<IEnumerator<InitiativeEvent>> enumerators = new List<IEnumerator<InitiativeEvent>>(_actors.Count);
            foreach (InitiativeEvent e in _actors)
            {
                enumerators.Add(e.NextActions.GetEnumerator());
            }
            
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    //public abstract class InitiativeEvent 
    //{
    //    public string Name { get; set; }
    //    public string TeamImage { get; set; }

    //    public object Clone()
    //    {
    //        return this.MemberwiseClone();
    //    }
    //}

    //public class Actor : InitiativeEvent
    //{
        
    //}

    //public class Spell : InitiativeEvent
    //{
    //}
}
