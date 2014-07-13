using IronText.Misc;
using System;
namespace IronText.Collections
{
    interface IIndexedCollection<T> where T : class, IHasIdentity
    {
        int IndexCount { get; }
        
        T this[int index] { get; set; }
    }
}
