using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Astral.Projector
{
    // todo: extract interface and make this remoteable
    public class PlayerViewController
    {
        public void SetVisible(bool isVisible)
        {
        }

        public void LoadMap(string mapPath)
        {
        }


        public void DisplayEffect(string effect)
        {
        }

        public void SetZoomLevel(int zoom)
        {
        }

        public void UpdateMapPosition(Point newPosition)
        {
        }

        //
        // Fog of War methods
        //

        // todo: handle different zoom levels
        public void UpdateFogAt(Point position, bool addFog)
        {
            
        }
    }
}
