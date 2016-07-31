namespace IronText.Collections
{
    public static class AmbiguousAlternativesExtensions
    {
        public static AmbiguousAlternatives<T> Alternatives<T>(this T @this)
            where T : Ambiguous<T>
        {
            return new AmbiguousAlternatives<T>(@this);
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
