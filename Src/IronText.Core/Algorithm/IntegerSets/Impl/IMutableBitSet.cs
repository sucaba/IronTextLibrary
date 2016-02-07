using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm.IntegerSets.Impl
{
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
