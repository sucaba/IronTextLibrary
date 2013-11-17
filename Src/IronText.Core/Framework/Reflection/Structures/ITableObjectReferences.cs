using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public interface ITableObjectReferences<T>
        where T : ITableObject
    {
        ITableObjectOwner<T> Owner { get; set; }
    }
}
