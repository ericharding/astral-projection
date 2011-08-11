using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Astral.Plane.Utility
{
    static class XAttributeExtensions
    {
        public static T Parse<T>(this XAttribute attribute, Type enumType, T def = default(T))
        {
            return attribute.Parse(s => (T)Enum.Parse(enumType, s), def);
        }

        public static T Parse<T>(this XAttribute attribute, Func<string, T> parse, T def = default(T))
        {
            if (attribute != null)
            {
                return parse(attribute.Value);
            }
            return def;
        }
    }
}
