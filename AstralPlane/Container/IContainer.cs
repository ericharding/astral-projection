using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Astral.Plane.Container
{
    interface IContainer : IDisposable
    {
        bool ContainsFile(string filename);
        Stream GetFileStream(string filename, bool create = true);
    }
}
