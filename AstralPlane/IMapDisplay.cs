using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections;
using Astral.Plane.Utility;

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

        IIndexable<int, bool> LayerMap { get; }

        Point PixelsToTiles(double x, double y);
        Point TilesToPixels(double x, double y);
    }
}
