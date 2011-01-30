using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Astral.Plane.Utility
{
    static class XAttributeExtensions
    {
        public static T Parse<T>(this XAttribute attribute, Func<string, T> parse)
        {
            if (attribute != null)
            {
                return parse(attribute.Value);
            }
            return default(T);
        }
    }
}
