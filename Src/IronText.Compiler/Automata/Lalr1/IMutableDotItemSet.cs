using System.Collections.Generic;
using IronText.Compiler.Analysis;

namespace IronText.Automata.Lalr1
{
    public interface IMutableDotItemSet
        : IDotItemSet
    {
        new DotItem this[int index] { get; set; }

        bool Add(DotItem item);

        void AddRange(IEnumerable<DotItem> items);
    }
}
