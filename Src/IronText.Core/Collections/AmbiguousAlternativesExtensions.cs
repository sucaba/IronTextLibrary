namespace IronText.Collections
{
    public static class AmbiguousAlternativesExtensions
    {
        public static AmbiguousAlternatives<T> Alternatives<T>(this T @this)
            where T : Ambiguous<T>
        {
            return new AmbiguousAlternatives<T>(@this);
        }
    }
}
