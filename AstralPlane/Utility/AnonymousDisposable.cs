using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astral.Plane.Utility
{
    internal class AnonymousDisposable : IDisposable
    {
        private Action _dispose;
        public AnonymousDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (_dispose != null)
                _dispose();
        }
    }
}
