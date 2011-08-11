using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astral.Plane
{
    class MarkerTileFactory : TileFactory
    {
        public MarkerTileFactory(Map map)
            : base(map, "Marker", string.Empty, "Marker", Borders.Empty, 1, 1, false)
        {
        }

        internal override void LoadBitmapSource()
        {
        }
    }
}
