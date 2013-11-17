using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public interface ITableObjectOwner<T>
        where T : ITableObject
    {
        void Add(T item);

        void Remove(T item);
    }
}
