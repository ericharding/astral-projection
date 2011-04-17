using System;
using System.Windows.Documents;
namespace Astral.Projector
{
    interface IAdventureLinkHandler
    {
        Span MakeHyperlink(string rawText);
        string Prefix { get; }
    }
}
