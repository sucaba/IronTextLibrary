using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronText.DI
{
    internal static class ObjectExtensions
    {
        public static T RequireNonNull<T>(this T instance)
            where T : class
        {
            if (instance == null)
            {
                throw new InvalidDependencyException(
                    $"Cannot create {typeof(T).Name}");
            }

            return instance;
        }
    }
}
