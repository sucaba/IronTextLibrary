using IronText.Algorithm.IntegerSets.Impl;
using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    [Serializable]
    public class BitSetBase : MutableIntSet
    {
        private MutableBitSetImpl impl;

        internal BitSetBase(IntSetType setType, int bitCount, bool defaultBit)
            : base(setType)
        {
            impl = new MutableBitSetImpl(bitCount, defaultBit);
        }

        internal BitSetBase(IntSetType setType, MutableBitSetImpl impl)
            : base(setType)
        {
            this.impl = impl;
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
            var casted = (BitSetBase)other;
            impl.AddAll(casted.impl);
        }

        public override void RemoveAll(IntSet other)
        {
            var casted = (BitSetBase)other;
            impl.RemoveAll(casted.impl);
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
            var casted = (BitSetBase)other;
            var resultImpl = impl.Union(casted.impl);
            return new BitSet(this.setType, resultImpl);
        }

        public override IntSet Intersect(IntSet other)
        {
            var casted = (BitSetBase)other;
            var resultImpl = impl.Intersect(casted.impl);
            return new BitSet(this.setType, resultImpl);
        }

        public override IntSet Complement(IntSet vocabulary)
        {
            var casted = (BitSetBase)vocabulary;
            var resultImpl = impl.Complement(casted.impl);
            return new BitSet(this.setType, resultImpl);
        }

        public override bool SetEquals(IntSet other)
        {
            var casted = (BitSetBase)other;
            return impl.Equals(casted.impl);
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
    }
}
