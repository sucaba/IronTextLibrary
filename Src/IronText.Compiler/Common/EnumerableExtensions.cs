using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Common
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> NonNull<T>(this IEnumerable<T> @this)
            where T : class =>
            @this.Where(x => x != null);

        public static IEnumerable<T> NonNull<T>(this IEnumerable<Nullable<T>> @this)
            where T : struct =>
            @this
                .Where(x => x.HasValue)
                .Select(x => x.Value);
    }
}
