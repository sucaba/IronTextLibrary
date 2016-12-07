using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronText.Common
{
    class ManyToMany<T> : Dictionary<T, List<T>>
    {
        public void Add(T from, T to)
        {
            List<T> tos;
            if (!TryGetValue(from, out tos))
            {
                tos = new List<T>();
                this.Add(from, tos);
            }

            tos.Add(to);
        }

        public IEnumerable<T> Get(T from)
        {
            List<T> result;
            return TryGetValue(from, out result)
                ? result
                : Enumerable.Empty<T>();
        }
    }
}
