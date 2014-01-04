using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Collections
{
    public interface IReferenceContainer<T>
    {
        IOwner<T> Owner { get; set; }
    }
}
