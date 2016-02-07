using IronText.Algorithm.IntegerSets.Impl;
using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    [Serializable]
    public class BitSetType : IntSetType
    {
        // Consider 1 Kb as a max memory allowed for bitset
        public const int MaxBits = 8 * 1024;

        private IntSet _all;
        private IntSet _empty;
        private int _bitCount;

        public BitSetType(int bitCount)
        {
            this.BitCount = bitCount;
        }

        public int BitCount
        { 
            get { return _bitCount; }
            private set 
            { 
                if (value > MaxBits)
                {
                    throw new ArgumentOutOfRangeException("bitCount", "bitCount is too big. Maximum is " + MaxBits);
                }

                _bitCount = value; 
                this._empty = new BitSet(this, value, false);
                this._all = new BitSet(this, value, true);
            }
        }

        public override int MinValue { get { return 0; } }

        public override int MaxValue { get { return _bitCount - 1; } }

        public override IntSet All { get { return _all; } }

        public override IntSet Empty { get { return _empty; } }

        public override IntSet Of(int value)
        {
            var impl = new MutableBitSetImpl(_bitCount, false);
            impl.Add(value);
            return new BitSet(this, impl);
        }

        public override IntSet Of(IEnumerable<int> value)
        {
            var impl = new MutableBitSetImpl(_bitCount, false);
            foreach (var item in value)
            {
                impl.Add(item);
            }

            return new BitSet(this, impl);
        }

        public override IntSet Range(int from, int to)
        {
            var impl = new MutableBitSetImpl(_bitCount, false);
            impl.Add(new IntInterval(from, to));
            return new BitSet(this, impl);
        }

        public override IntSet Ranges(IEnumerable<IntInterval> intervals)
        {
            if (intervals == null)
            {
                throw new ArgumentNullException("intervals");
            }

            var impl = new MutableBitSetImpl(_bitCount, false);
            foreach (var item in intervals)
            {
                impl.Add(item);
            }

            return new BitSet(this, impl);
        }

        public override MutableIntSet Mutable()
        {
            return new MutableBitSet(this, _bitCount, false);
        }
    }
}
