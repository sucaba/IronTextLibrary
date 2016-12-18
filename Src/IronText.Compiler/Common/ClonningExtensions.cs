using System.Collections.Generic;

namespace IronText.Common
{
    static class ClonningExtensions
    {
        public static Dictionary<TKey,TValue> DeepClone<TKey,TValue>(this Dictionary<TKey,TValue> @this)
        {
            return new Dictionary<TKey, TValue>(@this);
        }
    }
}
