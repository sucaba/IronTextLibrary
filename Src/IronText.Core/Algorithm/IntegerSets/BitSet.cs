
using IronText.Algorithm.IntegerSets.Impl;
using System;

namespace IronText.Algorithm
{
    [Serializable]
    public sealed class BitSet : BitSetBase
    {
        public BitSet(IntSetType setType, int bitCount, bool defaultBit)
            : base(setType, bitCount, defaultBit)
        {
        }

        internal BitSet(IntSetType setType, MutableBitSetImpl impl)
            : base(setType, impl)
        {
        }
    }
}
