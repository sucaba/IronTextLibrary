using IronText.Algorithm.IntegerSets.Impl;
using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    sealed class MutableBitSet : MutableIntSet, IHasImpl<MutableBitSetImpl>
    {
        private MutableBitSetImpl impl;

        public MutableBitSet(IntSetType setType, int bitCount, bool defaultBit)
            : this(setType, new MutableBitSetImpl(bitCount, defaultBit))
        {
        }

        internal MutableBitSet(IntSetType setType, MutableBitSetImpl impl)
            : base(setType)
        {
            this.impl = impl;
        }

        MutableBitSetImpl IHasImpl<MutableBitSetImpl>.Impl
        {
            get { return this.impl; }
        }

        public override void Add(int value)
        {
            impl.Add(value);
        }

        public override void Add(IntInterval interval)
        {
            impl.Add(interval);
        }

        public override int PopAny()
        {
            return impl.PopAny();
        }

        public override void Remove(int value)
        {
            impl.Remove(value);
        }

        public override void AddAll(IntSet other)
        {
            impl.AddAll(ImplOf(other));
        }

        public override void RemoveAll(IntSet other)
        {
            impl.RemoveAll(ImplOf(other));
        }

        public override IntSet CompleteAndDestroy()
        {
            var result = new BitSet(setType, impl);
            this.impl = null;
            return result;
        }

        public override bool Contains(int value) 
        {
            return impl.Contains(value);
        }

        public override bool IsEmpty { get { return impl.IsEmpty; } }

        public override int Count { get { return impl.Count; } }

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
