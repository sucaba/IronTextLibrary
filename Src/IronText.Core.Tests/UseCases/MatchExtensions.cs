using System;

namespace IronText.Tests.UseCases
{
    public static class MatchExtensions
    {
        public static V Match<V>(this Action<V> node, V visitor)
        {
            node(visitor);
            return visitor;
        }

        public static V Match<V>(this Action<V> node) where V : new()
        {
            var visitor = new V();
            node(visitor);
            return visitor;
        }
    }
}
