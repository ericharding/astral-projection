using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Astral.Projector.Initiative;
using System.Linq;

namespace Astral.Projector
{
    class AdventureTextFormatter
    {
        private InitiativeManager _initiativeManager;
        private Action<Uri> _uriNavigate;
        private Regex _linkParser = new Regex(@"\[\[.*?\]\]", RegexOptions.Compiled);


        public AdventureTextFormatter(InitiativeManager initiativeManager, Action<Uri> uriNavigate)
        {
            _initiativeManager = initiativeManager;
            FontFamily = new FontFamily("Segou UI");
            FontSize = 12;
            _uriNavigate = uriNavigate;
        }

        public FlowDocument MakeDocument(string notes)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = this.FontFamily;
            doc.FontSize = this.FontSize;

            //doc.ColumnWidth = 900;

            if (string.IsNullOrEmpty(notes))
                return doc;

            Paragraph para = new Paragraph();
            para.TextAlignment = TextAlignment.Left;

            foreach (string line in notes.Trim().Split('\n'))
            {
                AddLine(para, line);
                bool isEmpty = string.IsNullOrEmpty(line);

                // Attempt to split text into paragraphs maybe not so hot
                //if (isEmpty || Char.IsNumber(ling[0]))
                //{
                //    if (para.Inlines.Count != 0)
                //    {
                //        para.BreakPageBefore = !isEmpty;
                //        // doc.Blocks.Add(p);
                //        // p = new Paragraph();
                //    }
                //}
            }

            doc.Blocks.Add(para);
            return doc;
        }

        private void AddLine(Paragraph para, string s)
        {
            int linkIndex = s.IndexOf("[[");
            int linkIndexEnd = s.IndexOf("]]");
            if (linkIndex >= 0 &&
                linkIndexEnd >= 0 &&
                linkIndexEnd > linkIndex)
            {
                StringBuilder sb = new StringBuilder();

                var matches = _linkParser.Matches(s);

                int lastMatch = 0;
                for(int x=0; x<matches.Count; x++)
                {
                    para.Inlines.Add(s.Substring(lastMatch, matches[x].Index - lastMatch));
                    lastMatch = matches[x].Index + matches[x].Length;
                    
                    string linkText = matches[x].Value;

                    string eventText = linkText.Substring(2, linkText.Length-4);
                    CreateLink(para, eventText);
                }
                para.Inlines.Add(s.Substring(lastMatch, s.Length - lastMatch));
                para.Inlines.Add(Environment.NewLine);
            }
            else
            {
                para.Inlines.Add(s + Environment.NewLine);
            }
        }

        private void CreateLink(Paragraph para, string eventText)
        {
            Hyperlink link = null;
            try
            {
                if (eventText.StartsWith("spell:"))
                {
                    link = CreateSpellHyperlink(eventText.Substring("spell:".Length).Trim());
                }
                else if (eventText.StartsWith("link:"))
                {
                    link = MakeHyperlink(eventText.Substring("link:".Length).Trim());
                }
                else if (eventText.StartsWith("http://"))
                {
                    link = MakeHyperlink(eventText);
                }
                else
                {
                    Event parsedEvent = _initiativeManager.CreateEvent(eventText);
                    link = CreateInitiativeLink(eventText, parsedEvent);
                }

                para.Inlines.Add(link);
            }
            catch (Exception e) { para.Inlines.Add(e.ToString()); }

        }

        private Hyperlink MakeHyperlink(string text)
        {
            var split = new TextSplit(text, "(http:", ")");
            Hyperlink link = new Hyperlink();

            if (split.FoundMatch)
            {
                link.Inlines.Add(split.Before.Trim());
                link.NavigateUri = new Uri("http:"+split.Middle);
            }
            else
            {
                link.Inlines.Add(text);
                link.NavigateUri = new Uri(text);
            }

            link.Click += UrlNavigate;
            return link;
        }

        private Hyperlink CreateSpellHyperlink(string spellName)
        {
            Hyperlink link;
            link = new Hyperlink();

            if (spellName.IndexOf("(http") > 0)
            {
                return MakeHyperlink(spellName);
            }

            link.Inlines.Add(spellName);
            link.NavigateUri = new Uri(MakeSpellLink(spellName));
            link.Click += UrlNavigate;
            return link;
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

        private Hyperlink CreateInitiativeLink(string line, Event doodad)
        {
            Hyperlink hl = new Hyperlink();
            hl.Inlines.Add(doodad.Name);
            hl.ToolTip = line;
            hl.Click += new RoutedEventHandler(Link_Click);
            hl.Tag = line;
            return hl;
        }

        void UrlNavigate(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            _uriNavigate(link.NavigateUri);
        }

        void Link_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            string text = link.Tag as string;
            try
            {
                _initiativeManager.AddEvent(text);
            }
            catch
            {
                MessageBox.Show("Invalid.");
            }
        }

        
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
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
        public string After { get { return Original.Substring(End); }}
        public string Middle { get { return Original.Substring(insideStart, insideEnd - insideStart); } }
        public bool FoundMatch { get { return Start >= 0 && End > Start; } }
    }
}
