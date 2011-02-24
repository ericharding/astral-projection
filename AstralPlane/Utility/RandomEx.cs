using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astral.Plane.Utility
{
    public static class RandomEx
    {
        static Random _rand = new Random();
        public static Random Instance { get { return _rand; } }
    }
}
