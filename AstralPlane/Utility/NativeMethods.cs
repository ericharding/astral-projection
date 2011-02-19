using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Astral.Plane.Utility
{
    public class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
    }
}
