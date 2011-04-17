using System;
using System.Windows.Documents;
namespace Astral.Projector
{
    interface IAdventureLinkHandler
    {
        Hyperlink MakeHyperlink(string rawText);
        string Prefix { get; }
    }
}
