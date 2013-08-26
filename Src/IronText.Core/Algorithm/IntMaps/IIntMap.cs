using System.Collections.Generic;

namespace IronText.Algorithm
{
    using Int = System.Int32;

    public interface IIntMap<TAttr>
    {
        TAttr DefaultValue { get; }

        IntInterval Bounds { get; }

        TAttr Get(Int value);

        IEnumerable<IntArrow<TAttr>> Enumerate();

        IEnumerable<IntArrow<TAttr>> EnumerateCoverage(IntInterval bounds);
    }
}
