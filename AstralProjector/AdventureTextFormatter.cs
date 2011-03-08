﻿using System;
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
        private Regex _linkParser = new Regex(@"\[\[.*?\]\]", RegexOptions.Compiled);

        public AdventureTextFormatter(InitiativeManager initiativeManager)
        {
            _initiativeManager = initiativeManager;
            FontFamily = new FontFamily("Segou UI");
            FontSize = 12;
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

            foreach (string ling in notes.Trim().Split('\n'))
            {
                AddLine(para, ling);
                bool isEmpty = string.IsNullOrEmpty(ling);

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
                    Event parsedEvent = _initiativeManager.CreateEvent(eventText);
                    Hyperlink link = CreateHyperlink(eventText, parsedEvent);
                    para.Inlines.Add(link);
                }
                para.Inlines.Add(s.Substring(lastMatch, s.Length - lastMatch));
                para.Inlines.Add(Environment.NewLine);
            }
            else
            {
                para.Inlines.Add(s + Environment.NewLine);
            }
        }

        private Hyperlink CreateHyperlink(string line, Event doodad)
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
            _initiativeManager.AddEvent(text);
        }

        
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
    }
}