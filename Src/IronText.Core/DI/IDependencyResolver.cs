using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText
{
    internal interface IDependencyResolver
    {
        object Resolve(Type type);
    }

    internal static class DependecyResolverExtensions
    {
        public static T Resolve<T>(this IDependencyResolver self) where T : class
        {
            return (T)self.Resolve(typeof(T));
        }
    }
}
