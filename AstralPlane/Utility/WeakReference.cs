using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astral.Plane.Utility
{
    class WeakReference<T> : WeakReference
    {
        public WeakReference(object target)
            : base(target)
        {
        }

        public new T Target
        {
            get
            {
                return (T)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }
    }
}
