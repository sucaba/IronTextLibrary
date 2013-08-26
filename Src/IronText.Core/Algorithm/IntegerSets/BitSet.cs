
namespace IronText.Algorithm
{
    public sealed class BitSet : BitSetBase
    {
        public BitSet(IntSetType setType, int bitCount, bool defaultBit)
            : base(setType, bitCount, defaultBit)
        {
        }

        public BitSet(IntSetType setType, uint[] words)
            : base(setType, words)
        {
        }
    }
}
