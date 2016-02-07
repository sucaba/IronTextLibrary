
using IronText.Algorithm.IntegerSets.Impl;

namespace IronText.Algorithm
{
    public sealed class MutableBitSet : BitSetBase
    {
        public MutableBitSet(IntSetType setType, int bitCount, bool defaultBit)
            : base(setType, bitCount, defaultBit)
        {
        }

        internal MutableBitSet(IntSetType setType, MutableBitSetImpl impl)
            : base(setType, impl)
        {
        }
    }
}
