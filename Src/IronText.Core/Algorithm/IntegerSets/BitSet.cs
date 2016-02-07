
using IronText.Algorithm.IntegerSets.Impl;
using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    [Serializable]
    sealed class BitSet : IntSet, IHasImpl<MutableBitSetImpl>
    {
        private MutableBitSetImpl impl;

        public BitSet(IntSetType setType, int bitCount, bool defaultBit)
            : this(setType, new MutableBitSetImpl(bitCount, defaultBit))
        {
        }

        internal BitSet(IntSetType setType, MutableBitSetImpl impl)
            : base(setType)
        {
            this.impl = impl;
        }

        public override bool Contains(int value) 
        {
            return impl.Contains(value);
        }

        public override bool IsEmpty { get { return impl.IsEmpty; } }

        public override int Count { get { return impl.Count; } }

        MutableBitSetImpl IHasImpl<MutableBitSetImpl>.Impl
        {
            get { return this.impl; }
        }

        public override IntSet Union(IntSet other)
        {
            var resultImpl = impl.Union(ImplOf(other));
            return new BitSet(this.setType, resultImpl);
        }

        public override IntSet Intersect(IntSet other)
        {
            var resultImpl = impl.Intersect(ImplOf(other));
            return new BitSet(this.setType, resultImpl);
        }

        public override IntSet Complement(IntSet vocabulary)
        {
            var resultImpl = impl.Complement(ImplOf(vocabulary));
            return new BitSet(this.setType, resultImpl);
        }

        public override bool SetEquals(IntSet other)
        {
            return impl.Equals(ImplOf(other));
        }

        public override IntSet Clone()
        {
            return new BitSet(setType, impl.Clone());
        }

        public override int Min()
        {
            throw new NotImplementedException();
        }

        public override int Max()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<int> GetEnumerator()
        {
            return impl.GetEnumerator();
        }

        public override MutableIntSet EditCopy()
        {
            return new MutableBitSet(setType, impl.Clone());
        }

        public override IEnumerable<IntInterval> EnumerateIntervals()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return impl.GetHashCode();
        }

        public override string ToCharSetString()
        {
            throw new NotImplementedException();
        }

        private static MutableBitSetImpl ImplOf(IntSet adapter)
        {
            return ((IHasImpl<MutableBitSetImpl>)adapter).Impl;
        }
    }
}
