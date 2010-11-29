using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astral.Projector
{
    interface IPlayerView
    {

        bool IsVisible { get; set; }

        // Interface?
        FogOfWar FogOfWar { get; set; }

        void SetMapPosition(int x, int y);
    }
}
