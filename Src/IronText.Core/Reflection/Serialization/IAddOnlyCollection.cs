using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal interface IAddOnlyCollection<T> : IEnumerable<T>
    {
        void Add(T item);
    }
}
