using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm.IntegerSets.Impl
{
    interface IMutableBitSet<TDerived>  : IBitSet<TDerived>
    {
        int Add(IntInterval interval);

        bool Add(int value);

        int PopAny();

        void Remove(int value);

        int AddAll(TDerived other);

        void RemoveAll(TDerived other);
    }
}
