using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronText.Common
{
    static class EnumerableExtensions
    {
        public static IEnumerable<T> EnumerateGrowable<T>(this IList<T> @this)
        {
            int count = @this.Count;
            for (int i = 0; i != count; ++i)
            {
                yield return @this[i];
            }
        }
    }
}
