using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Collections
{
    public interface IReferenceContainer<T>
    {
        IOwner<T> Owner { get; set; }
    }
}
