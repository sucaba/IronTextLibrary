namespace IronText.Collections
{
    public abstract class Ambiguous<T>
        where T : Ambiguous<T>
    {
        protected Ambiguous(T alternative = null)
        {
            Alternative = alternative;
        }

        public T Alternative { get; set; }

        public bool IsDeterminisic => Alternative == null;
    }
}
