using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astral.Plane.Utility
{
    public interface IIndexable<T1, T2>
    {
        T2 this[T1 index] { get; set; }
    }
}
