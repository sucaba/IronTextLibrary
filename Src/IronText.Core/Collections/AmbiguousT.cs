namespace IronText.Collections
{
    public abstract class Ambiguous<T>
        where T : Ambiguous<T>
    {
        public static readonly T NoAlternatives = null;

        protected Ambiguous(T alternative = null)
        {
            Alternative = alternative;
        }

        public T    Alternative    { get; set; }

        public bool IsDeterminisic => Alternative == null;

        public bool IsAmbiguous    => Alternative != null;
    }
}
