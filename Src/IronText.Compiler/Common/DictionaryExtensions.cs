using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronText.Common
{
    static class DictionaryExtensions
    {
        public static T GetOrDefault<TKey, T>(this Dictionary<TKey, T> @this, TKey key) =>
            @this.GetOrDefault(key, default(T));

        public static T GetOrDefault<TKey,T>(this Dictionary<TKey,T> @this, TKey key, Func<T> @default)
        {
            T result;
            if (!@this.TryGetValue(key, out result))
            {
                result = @default();
            }

            return result;
        }

        public static T GetOrAdd<TKey,T>(this Dictionary<TKey,T> @this, TKey key, T @default)
        {
            return @this.GetOrAdd(key, () => @default);
        }

        public static T GetOrAdd<TKey,T>(this Dictionary<TKey,T> @this, TKey key, Func<T> @default)
        {
            T result;
            if (!@this.TryGetValue(key, out result))
            {
                result = @default();
                @this.Add(key, result);
            }

            return result;
        }


        public static T GetOrDefault<TKey,T>(this Dictionary<TKey,T> @this, TKey key, T @default)
        {
            T result;
            if (!@this.TryGetValue(key, out result))
            {
                result = @default;
            }

            return result;
        }
    }
}
