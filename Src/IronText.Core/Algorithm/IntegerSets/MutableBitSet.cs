
namespace IronText.Algorithm
{
    public sealed class MutableBitSet : BitSetBase
    {
        public MutableBitSet(IntSetType setType, int bitCount, bool defaultBit)
            : base(setType, bitCount, defaultBit)
        {
        }

        public MutableBitSet(IntSetType setType, uint[] words)
            : base(setType, words)
        {
        }
    }
}
