using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Astral.Plane
{
    public interface IMapDisplay
    {
        void SetMap(Map map);

        event Action<long, long> MapPositionChanged;

        void SetMapPosition(long X, long Y);

        Size MapDimensions { get; }
    }
}
