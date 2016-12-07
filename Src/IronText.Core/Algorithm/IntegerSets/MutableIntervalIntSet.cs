using IronText.Algorithm.IntegerSets.Impl;
using System.Collections.Generic;
using System;

namespace IronText.Algorithm
{
    sealed internal class MutableIntervalIntSet 
        : MutableIntSet
        , IHasImpl<MutableIntervalIntSetImpl>
    {
        private MutableIntervalIntSetImpl impl;

        public MutableIntervalIntSet(IntSetType setType)
            : this(setType, new MutableIntervalIntSetImpl())
        {
        }

        public MutableIntervalIntSet(IntSetType setType, MutableIntervalIntSetImpl impl)
            : base(setType)
        {
            this.impl = impl;
        }

        public override int Count { get { return impl.Count; } }

        public override bool IsEmpty { get { return impl.IsEmpty; } }

        public override int Add(IntInterval interval)
        {
            return impl.Add(interval);
        }

        public override bool Add(int value)
        {
            return impl.Add(value);
        }

        public override int AddAll(IntSet other)
        {
            return impl.AddAll(ImplOf(other));
        }

        public override IntSet Clone()
        {
            return new IntervalIntSet(setType, impl.Clone());
        }

        public override IntSet Complement(IntSet vocabulary)
        {
            return new IntervalIntSet(setType, impl.Complement(ImplOf(vocabulary)));
        }

        public override IntSet CompleteAndDestroy()
        {
            var result = new IntervalIntSet(setType, impl);
            this.impl = null;
            return result;
        }

        public override bool Contains(int value)
        {
            return impl.Contains(value);
        }

        public override MutableIntSet EditCopy()
        {
            return new MutableIntervalIntSet(setType, impl.Clone());
        }

        public override IEnumerable<IntInterval> EnumerateIntervals()
        {
            return impl.EnumerateIntervals();
        }

        public override IEnumerator<int> GetEnumerator()
        {
            return impl.GetEnumerator();
        }

        public override IntSet Intersect(IntSet other)
        {
            return new IntervalIntSet(setType, impl.Intersect(ImplOf(other)));
        }

        public override int Max()
        {
            throw new NotImplementedException();
        }

        public override int Min()
        {
            throw new NotImplementedException();
        }

        public override int PopAny()
        {
            return impl.PopAny();
        }

        public override void Remove(int value)
        {
            impl.Remove(value);
        }

        public override void RemoveAll(IntSet other)
        {
            impl.RemoveAll(ImplOf(other));
        }

        public override bool SetEquals(IntSet other)
        {
            return impl.Equals(ImplOf(other));
        }

        protected override int DoGetIntSetHash()
        {
            return impl.GetHashCode();
        }

        public override string ToCharSetString()
        {
            return impl.ToCharSetString();
        }

        public override IntSet Union(IntSet other)
        {
            return new IntervalIntSet(setType, impl.Union(ImplOf(other)));
        }

        MutableIntervalIntSetImpl IHasImpl<MutableIntervalIntSetImpl>.Impl
        {
            get { return impl; }
        }

        private static MutableIntervalIntSetImpl ImplOf(IntSet adapter)
        {
            return ((IHasImpl<MutableIntervalIntSetImpl>)adapter).Impl;
        }
    }
}
