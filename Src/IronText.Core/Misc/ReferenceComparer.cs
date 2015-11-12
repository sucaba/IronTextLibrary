using System.Collections.Generic;

namespace IronText.Misc
{
    internal class ReferenceComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public static readonly ReferenceComparer<T> Default = new ReferenceComparer<T>();

        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
