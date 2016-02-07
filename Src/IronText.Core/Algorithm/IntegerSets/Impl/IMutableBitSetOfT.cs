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

    interface IMutableBitSet<TDerived>  : IBitSet<TDerived>
    {
        void Add(IntInterval interval);

        void Add(int value);

        int PopAny();

        void Remove(int value);

        void AddAll(TDerived other);

        void RemoveAll(TDerived other);
    }
}