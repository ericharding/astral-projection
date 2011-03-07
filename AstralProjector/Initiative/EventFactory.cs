using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Astral.Projector.Initiative
{
    internal class EventFactory
    {
        #region static
        static Regex _reSearch = new Regex(@"\w+:", RegexOptions.Compiled);
        static Regex _reIsDice = new Regex(@"(?<count>\d+)(d(?<die>\d+))?((?<operator>[+\-\*\/])(?<operand>\d+))?", RegexOptions.Compiled);
        static Dictionary<string, string> _alternatives = new Dictionary<string, string>();
        static Random _rand = new Random();

        static EventFactory()
        {
            _alternatives.Add("health", "hp");
            _alternatives.Add("hd", "hp");
            _alternatives.Add("duration", "dur");
        }

        #endregion

        InitiativeManager _initMgr;
        public EventFactory(InitiativeManager mgr)
        {
            _initMgr = mgr;
        }

        public Event Create(string desc)
        {
            var matches = _reSearch.Matches(desc);

            if (matches.Count == 0)
            {
                return CreateSimple(desc);
            }
            
            Dictionary<string, string> properties = new Dictionary<string, string>();

            string name = desc.Substring(0, matches[0].Index).Trim();
            for (int x = 1; x <= matches.Count; x++)
            {
                
                string key = desc.Substring(matches[x-1].Index, matches[x-1].Length-1);
                int valueStart = matches[x-1].Index + matches[x-1].Length;
                int end = x < matches.Count ? matches[x].Index : desc.Length;
                string value = desc.Substring(valueStart, end - valueStart).Trim();
                if (_alternatives.ContainsKey(key))
                {
                    key = _alternatives[key];
                }
                properties[key] = value;
            }

            // Special case for Orc 3d8 ac:12
            int lastSpace = name.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                string lastToken = name.Substring(lastSpace);
                if (!properties.ContainsKey("hp") && _reIsDice.IsMatch(lastToken))
                {
                    name = name.Substring(0, lastSpace);
                    properties["hp"] = lastToken;
                }
            }

            return CreateComplex(name, properties);
        }

        private Event CreateComplex(string name, Dictionary<string, string> properties)
        {
            Team team = _initMgr.CurrentTeam;
            bool isActor = properties.ContainsKey("hp");
            if (isActor)
            {
                int health = Roll(properties["hp"]);
                if (properties.ContainsKey("team"))
                {
                    team = ParseTeam(properties["team"]);
                    properties.Remove("team");
                }
                properties.Remove("hp");
                return new Actor(name, team, health, _initMgr) { Properties = properties };
            }
            else
            {
                if (!properties.ContainsKey("dur"))
                {
                    throw new EventFactoryException("Spell effects must have a duration");
                }

                int numRounds = Roll(properties["dur"]);
                properties.Remove("dur");
                TimeSpan duration = TimeSpan.FromSeconds(numRounds * InitiativeManager.FULLROUND_SECONDS);
                return new SpellEffect(name, duration, _initMgr) { Properties = properties };
            }
        }

        private static Team ParseTeam(string teamName)
        {
            Team result;
            int teamNum;
            if (Enum.TryParse<Team>(teamName, out result))
            {
                return result;
            }
            else if (Int32.TryParse(teamName, out teamNum))
            {
                return (Team)teamNum;
            }
            else
            {
                throw new EventFactoryException("Team what now?");
            }
        }

        private Event CreateSimple(string desc)
        {
            // short circut syntax:  Joe 2d8 => Joe hp:2d8
            int lastSpace = desc.LastIndexOf(' ');
            string name = desc.Substring(0, lastSpace).Trim();
            string hp = desc.Substring(lastSpace, desc.Length - lastSpace).Trim();
            return new Actor(name, _initMgr.CurrentTeam, Roll(hp), _initMgr);
        }

        public static int Roll(string diceExpression)
        {
            if (diceExpression.Contains('d'))
            {
                var match = _reIsDice.Match(diceExpression.Trim());
                int count = Int32.Parse(match.Groups["count"].Value);
                int die = Int32.Parse(match.Groups["die"].Value);

                char op = '+';
                if (!string.IsNullOrEmpty(match.Groups["operator"].Value))
                {
                    op = match.Groups["operator"].Value[0];
                }

                int operand = 0;
                if (!string.IsNullOrEmpty(match.Groups["operand"].Value))
                {
                    operand = Int32.Parse(match.Groups["operand"].Value);
                }

                int total = 0;
                for (int x = 0; x < count; x++)
                {
                    total += _rand.Next(die) + 1;
                }
                switch (op)
                {
                    case '+':
                        total += operand;
                        break;
                    case '-':
                        total = Math.Max(1, total-operand);
                        break;
                    case '*':
                        total *= operand;
                        break;
                    case '/':
                        total = Math.Max(1, total / operand);
                        break;
                }

                return total;
            }
            else
            {
                return Int32.Parse(diceExpression);
            }
        }
    }

    [Serializable]
    internal class EventFactoryException : Exception
    {
        public EventFactoryException(string message)
            : base(message)
        { }
    }
}
