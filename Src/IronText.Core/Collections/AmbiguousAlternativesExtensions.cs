using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Collections
{
    public static class AmbiguousAlternativesExtensions
    {
        public static U MapAltenatives<T, U>(this T @this, Func<T, U> convert)
            where T : Ambiguous<T>
            where U : Ambiguous<U> =>
            @this
                .AllAlternatives()
                .Select(convert)
                .AsAmbiguous();

        public static T AsAmbiguous<T>(this IEnumerable<T> @this)
            where T : Ambiguous<T> =>
            @this.Aggregate(Ambiguous<T>.NoAlternatives, Alternate);

        public static AmbiguousAlternatives<T> AllAlternatives<T>(this T @this)
            where T : Ambiguous<T>
        {
            return new AmbiguousAlternatives<T>(@this);
        }

        public static AmbiguousAlternatives<T> OtherAlternatives<T>(this T @this)
            where T : Ambiguous<T>
        {
            return new AmbiguousAlternatives<T>(@this.Alternative);
        }

        public static T Alternate<T>(this T @this, T other)
            where T : Ambiguous<T>
        {
            var last = other.Last();
            last.Alternative = @this;

            return other;
        }

        private static T Last<T>(this T @this)
            where T : Ambiguous<T>
        {
            var result = @this;
            while (result.Alternative != null)
            {
                result = result.Alternative;
            }

            return result;
        }
    }
}
