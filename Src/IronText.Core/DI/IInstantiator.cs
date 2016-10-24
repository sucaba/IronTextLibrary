using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.DI
{
    public interface IInstantiator
    {
        object Execute(Type resultType, Func<object> factory);
    }

    public static class DependencyFactoryInterceptorExtensions
    {
        public static T Execute<T>(
            this IInstantiator @this,
            Func<T> factory)
        {
            return (T)@this.Execute(typeof(T), () => factory());
        }
    }
}
