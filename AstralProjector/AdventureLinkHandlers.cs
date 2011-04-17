using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Text.RegularExpressions;

namespace Astral.Projector
{
    class AdventureLinkHandler<T> : IAdventureLinkHandler
    {
        protected Action<T> _action;

        public AdventureLinkHandler(string prefix, Action<T> action)
        {
            Prefix = prefix + ":";
            _action = action;
        }

        public string Prefix { get; private set; }
        public void Invoke(object sender, EventArgs args)
        {
            Hyperlink link = sender as Hyperlink;
            T parameter = (T)link.CommandParameter;
            _action(parameter);
        }

        protected virtual Tuple<string, T> ParseLink(string raw)
        {
            return Tuple.Create(raw, default(T));
        }

        public Hyperlink MakeHyperlink(string rawText)
        {
            Hyperlink link = new Hyperlink();
            link.Click += Invoke;
            var linkdata = ParseLink(rawText.Substring(Prefix.Length).Trim());
            link.Inlines.Add(linkdata.Item1.Trim());
            link.CommandParameter = linkdata.Item2;
            return link;
        }
    }

    class BasicAdventureLinkHandler : AdventureLinkHandler<string>
    {
        public BasicAdventureLinkHandler(string prefix, Action<string> action)
            : base(prefix, action) {}
    }

    class AdventureLinkWithParens : AdventureLinkHandler<string>
    {
        public AdventureLinkWithParens(string prefix, Action<string> action)
        : base(prefix, action)
        {}

        protected override Tuple<string, string> ParseLink(string raw)
        {
            var split = new TextSplit(raw, "(", ")");
            if (split.FoundMatch)
            {
                return Tuple.Create(split.Before.Trim(), split.Middle.Trim());
            }
            else
            {
                return base.ParseLink(raw);
            }
        }
    }

    class SpellLinkHandler : AdventureLinkWithParens
    {
        public SpellLinkHandler(string prefix, Action<string> action) : base(prefix, action) { }

        protected override Tuple<string, string> ParseLink(string raw)
        {
            if (raw.IndexOf("(") > 0)
            {
                return base.ParseLink(raw);
            }
            else
            {
                return Tuple.Create(raw, MakeSpellLink(raw));
            }
        }

        private string MakeSpellLink(string eventText)
        {
            // "Inflict serious wounds, mass" => http://digitalsorcery.net/d20/srd/spells/inflictSeriousWoundsMass.htm
            var tokens = eventText.ToLower().Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 1) return string.Empty;

            // Upper case each word
            string spell =
                char.ToLower(tokens[0][0]) // Lower case the first word
              + tokens[0].Substring(1)
              + string.Join("", tokens.Skip(1).Select(s => char.ToUpper(s[0]) + s.Substring(1))); // capitalize all other words

            return @"http://digitalsorcery.net/d20/srd/spells/" + spell + ".htm";
        }
    }

    class LayerLinkHandler : AdventureLinkHandler<Tuple<View, int>>
    {
        Regex _layerParser = new Regex(@"(player|dm )?(\d+)");

        public LayerLinkHandler(Action<View, int> toggle)
            : base("layer", tupple => toggle(tupple.Item1, tupple.Item2))
        {
        }

        protected override Tuple<string, Tuple<View, int>> ParseLink(string raw)
        {
            string lower = raw.ToLower();
            View view = lower.Contains("dm") ? View.DM : View.Player;
            var match = _layerParser.Match(lower);
            if (match == null) return base.ParseLink(raw);

            var layer = Tuple.Create(view, Int32.Parse(match.Groups[0].Value));
            return Tuple.Create("Layer "+raw, layer);
        }

    }

    public class TextSplit
    {
        private int insideStart, insideEnd;

        public TextSplit(string original, string startPattern, string endPattern)
        {
            this.Start = original.IndexOf(startPattern);
            insideStart = Start + startPattern.Length;
            insideEnd = original.IndexOf(endPattern, Start);
            End = insideEnd + endPattern.Length;
            this.Original = original;
        }

        public int Start { get; set; }
        public int End { get; set; }
        public string Original { get; set; }
        public string Before { get { return Original.Substring(0, Start); } }
        public string After { get { return Original.Substring(End); } }
        public string Middle { get { return Original.Substring(insideStart, insideEnd - insideStart); } }
        public bool FoundMatch { get { return Start >= 0 && End > Start; } }
    }
}
