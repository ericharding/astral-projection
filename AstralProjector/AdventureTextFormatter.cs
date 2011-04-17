using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Astral.Projector.Initiative;

namespace Astral.Projector
{
    class AdventureTextFormatter
    {
        private InitiativeManager _initiativeManager;
        IEnumerable<IAdventureLinkHandler> _linkActions;
        private Regex _linkParser = new Regex(@"\[\[.*?\]\]", RegexOptions.Compiled);


        public AdventureTextFormatter(InitiativeManager initiativeManager, params IAdventureLinkHandler[] actions)
        {
            _initiativeManager = initiativeManager;
            FontFamily = new FontFamily("Segou UI");
            FontSize = 12;
            _linkActions = actions;
        }

        public FlowDocument MakeDocument(string notes)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = this.FontFamily;
            doc.FontSize = this.FontSize;

            if (string.IsNullOrEmpty(notes))
                return doc;

            Paragraph para = new Paragraph();
            para.TextAlignment = TextAlignment.Left;

            foreach (string line in notes.Trim().Split('\n'))
            {
                AddLine(para, line);
                bool isEmpty = string.IsNullOrEmpty(line);
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
            Span link = null;
            try
            {
                foreach(var action in _linkActions)
                {
                    if (eventText.StartsWith(action.Prefix))
                    {
                        link = action.MakeHyperlink(eventText);
                        break;
                    }
                }
                
                if (link == null)
                {
                    Event parsedEvent = _initiativeManager.CreateEvent(eventText);
                    link = CreateInitiativeLink(eventText, parsedEvent);
                }

                para.Inlines.Add(link);
            }
            catch (Exception e) { para.Inlines.Add(e.ToString()); }
        }

        private Span CreateInitiativeLink(string line, Event doodad)
        {
            Hyperlink hl = new Hyperlink();
            hl.Inlines.Add(doodad.Name);
            hl.ToolTip = line;
            hl.Click += new RoutedEventHandler(Link_Click);
            hl.Tag = line;
            return hl;
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


}
