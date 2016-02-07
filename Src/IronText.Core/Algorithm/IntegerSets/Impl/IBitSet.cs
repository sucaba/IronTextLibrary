using System;
using System.Collections.Generic;

namespace IronText.Algorithm.IntegerSets.Impl
{
    interface IBitSet<TDerived> 
        : IEquatable<TDerived>
        , IEnumerable<int>
    {
        int  Count   { get; }

        bool IsEmpty { get; }

        bool Contains(int value);

        TDerived Clone();

        TDerived Complement(TDerived vocabulary);

        TDerived Union(TDerived other);

        TDerived Intersect(TDerived other);
    }
}