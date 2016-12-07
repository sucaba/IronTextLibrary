using System.Collections.Generic;

namespace IronText.Common
{
    static class ListExtensions
    {
        public static IEnumerable<T> EnumerateGrowable<T>(this List<T> @this)
        {
            for (int i = 0; i != @this.Count; ++i)
            {
                yield return @this[i];
            }
        }
    }
}
