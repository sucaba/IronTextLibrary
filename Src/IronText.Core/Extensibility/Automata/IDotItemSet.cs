using System;
using System.Collections.Generic;

namespace IronText.Extensibility
{
    public interface IDotItemSet 
        : IEnumerable<DotItem>
        , IEquatable<IDotItemSet>
    {
        int Count { get; }

        DotItem this[int index] { get; }

        int IndexOf(DotItem item);
    }
}
