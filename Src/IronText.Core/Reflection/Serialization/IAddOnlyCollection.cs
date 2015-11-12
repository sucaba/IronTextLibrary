using System.Collections.Generic;

namespace IronText.Reflection
{
    internal interface IAddOnlyCollection<T> : IEnumerable<T>
    {
        void Add(T item);
    }
}
