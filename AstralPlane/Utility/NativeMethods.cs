using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Astral.Plane.Utility
{
    internal class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern bool AllocConsole();
    }
}
