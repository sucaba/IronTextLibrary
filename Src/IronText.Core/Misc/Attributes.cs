using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronText.Misc
{
    internal static class Attributes
    {
        public static bool Exists(this ICustomAttributeProvider provider, Type attributeType)
        {
            return provider.GetCustomAttributes(attributeType, false).Any();
        }

        public static bool Exists<T>(this ICustomAttributeProvider provider)
        {
            return provider.GetCustomAttributes(typeof(T), false).Any();
        }

        public static T First<T>(this ICustomAttributeProvider provider) where T : class
        {
            foreach (T attr in provider.GetCustomAttributes(typeof(T), false))
            {
                return attr;
            }

            return null;
        }

        public static IEnumerable<T> All<T>(this ICustomAttributeProvider provider)
        {
            return provider.GetCustomAttributes(typeof(T), true).Cast<T>().ToArray();
        }
    }
}
