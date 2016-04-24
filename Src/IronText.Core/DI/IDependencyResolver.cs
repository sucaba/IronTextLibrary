using System;

namespace IronText
{
    internal interface IDependencyResolver
    {
        object Get(Type type);
    }

    internal static class DependecyResolverExtensions
    {
        public static T Get<T>(this IDependencyResolver self) where T : class
        {
            return (T)self.Get(typeof(T));
        }
    }
}
