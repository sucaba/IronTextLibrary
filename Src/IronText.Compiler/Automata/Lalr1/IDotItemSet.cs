using System;
using System.Collections.Generic;
using IronText.Automata.DotNfa;

namespace IronText.Automata.Lalr1
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
