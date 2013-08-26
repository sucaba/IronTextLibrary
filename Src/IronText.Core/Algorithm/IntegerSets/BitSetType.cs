using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
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

        public override IntSet All{ get { return _all; } }

        public override IntSet Empty { get { return _empty; } }

        public override IntSet Of(int value)
        {
            var result = new MutableBitSet(this, _bitCount, false);
            result.Add(value);
            return result;
        }

        public override IntSet Of(IEnumerable<int> value)
        {
            var result = new MutableBitSet(this, _bitCount, false);
            foreach (var item in value)
            {
                result.Add(item);
            }

            return result;
        }

        public override IntSet Range(int from, int to)
        {
            var result = new MutableBitSet(this, _bitCount, false);
            result.Add(new IntInterval(from, to));
            return result;
        }

        public override IntSet Ranges(IEnumerable<IntInterval> intervals)
        {
            if (intervals == null)
            {
                throw new ArgumentNullException("intervals");
            }

            var result = new MutableBitSet(this, _bitCount, false);
            foreach (var item in intervals)
            {
                result.Add(item);
            }

            return result;
        }

        public override MutableIntSet Mutable()
        {
            return new MutableBitSet(this, _bitCount, false);
        }
    }
}
