using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astral.Projector.Initiative
{
    public class InitiativeEvent : IComparable
    {
        public string Name { get; set; }
        public string TeamImage { get; set; }
        public DateTime NextAction { get; private set; }

        public IEnumerable<DateTime> NextActions
        {
            get
            {
                for(int x=0; x<=TurnManager.FUTURE_TURNS; x++)
                {
                    yield return NextAction + TimeSpan.FromSeconds(TurnManager.FULLROUND_SECONDS * x);
                }
            }
        }

        public int CompareTo(object obj)
        {
            InitiativeEvent other = obj as InitiativeEvent;
            if (other == null) return -1;

            return this.NextAction.CompareTo(other.NextAction);
        }
    }
}
