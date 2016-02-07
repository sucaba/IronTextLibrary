using IronText.Algorithm.IntegerSets.Impl;
using System;
using System.Collections.Generic;
using Int = System.Int32;

namespace IronText.Algorithm
{
    [Serializable]
    public sealed class IntervalIntSet
        : IntSet
        , IHasImpl<MutableIntervalIntSetImpl>
    {
        private MutableIntervalIntSetImpl impl;

        internal IntervalIntSet(IntSetType setType, MutableIntervalIntSetImpl impl)
            : base(setType)
        {
            this.impl = impl;
        }

        /// <summary>
        /// Create empty set
        /// </summary>
        /// <param name="setType"></param>
        internal IntervalIntSet(IntSetType setType)
            : this(setType, new MutableIntervalIntSetImpl())
        {
        }

        internal IntervalIntSet(IntSetType setType, IEnumerable<Int> enumerated)
            : this(setType)
        {
            foreach (var item in enumerated)
            {
                impl.Add(item);
            }
        }

        internal IntervalIntSet(IntSetType setType, Int first, Int last)
            : this(setType)
        {
            impl.Add(new IntInterval(first, last));
        }

        internal IntervalIntSet(IntSetType setType, IEnumerable<IntInterval> intervals)
            : this(setType)
        {
            if (intervals == null)
            {
                throw new ArgumentNullException("intervals");
            }

            foreach (var interval in intervals)
            {
                impl.Add(interval);
            }
        }

        MutableIntervalIntSetImpl IHasImpl<MutableIntervalIntSetImpl>.Impl
        {
            get { return impl; }
        }

        public override bool IsEmpty { get { return impl.IsEmpty; }
        }

        public override int Count { get { return impl.Count; } }

        public override bool Contains(int value)
        {
            return impl.Contains(value);
        }

        public override IntSet Union(IntSet other)
        {
            var resultImpl = this.impl.Union(ImplOf(other));
            return new IntervalIntSet(setType, resultImpl);
        }

        public override IntSet Intersect(IntSet other)
        {
            var resultImpl = impl.Intersect(ImplOf(other));
            return new IntervalIntSet(setType, resultImpl);
        }

        public override IntSet Complement(IntSet vocabulary)
        {
            var resultImpl = impl.Complement(ImplOf(vocabulary));
            return new IntervalIntSet(setType, resultImpl);
        }

        public override bool SetEquals(IntSet other)
        {
            return impl.Equals(ImplOf(other));
        }

        protected override int DoGetIntSetHash()
        {
            return impl.GetHashCode();
        }

        public override IntSet Clone()
        {
            return new IntervalIntSet(setType, impl.Clone());
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
            return new MutableIntervalIntSet(setType, impl.Clone());
        }

        public override IEnumerable<IntInterval> EnumerateIntervals()
        {
            return impl.EnumerateIntervals();
        }

        public override string ToCharSetString() 
        { 
            return impl.ToCharSetString(); 
        }

        private static MutableIntervalIntSetImpl ImplOf(IntSet adapter)
        {
            return ((IHasImpl<MutableIntervalIntSetImpl>)adapter).Impl;
        }
    }
}
