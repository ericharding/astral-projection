using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections;

namespace Astral.Plane
{
    public interface IMapDisplay
    {
        void SetMap(Map map);
        event Action MapChanged;

        event Action<long, long> MapPositionChanged;
        void SetMapPosition(long X, long Y);

        Rect MapBounds { get; }
        Rect MapViewport { get; }
        int TileSize { get; set; }

        event Action<int> TileSizeChanged;

        double ActualWidth { get; }
        double ActualHeight { get; }

        BitArray LayerMap { get; }
    }
}
