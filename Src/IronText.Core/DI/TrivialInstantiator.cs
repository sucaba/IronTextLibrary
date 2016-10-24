using System;

namespace IronText.DI
{
    class TrivialInstantiator : IInstantiator
    {
        public static readonly IInstantiator Instance = new TrivialInstantiator();

        public object Execute(Type resultType, Func<object> factory)
        {
            return factory();
        }
    }
}