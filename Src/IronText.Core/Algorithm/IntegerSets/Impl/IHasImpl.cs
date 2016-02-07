using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm.IntegerSets.Impl
{
    interface IHasImpl<TImpl>
    {
        TImpl Impl { get; }
    }
}
