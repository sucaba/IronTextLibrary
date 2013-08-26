using System.Collections.Generic;
using Int = System.Int32;

namespace IronText.Algorithm
{
    public class SparseIntSetType : IntSetType
    {
        private readonly IntSet empty;

        public static readonly SparseIntSetType Instance = new SparseIntSetType();

        protected SparseIntSetType()
        {
            this.empty = new IntervalIntSet(this);
        }

        public override Int MinValue { get { return 0; } }
        public override Int MaxValue { get { return Int.MaxValue - 10; } }

        public override IntSet All { get { return Range(MinValue, MaxValue); } }

        public override IntSet Empty { get { return empty; } }

        public override IntSet Of(Int value) { return Range(value, value); }
        public override IntSet Of(IEnumerable<Int> values) { return new IntervalIntSet(this, values); }

        public override IntSet Range(Int from, Int to) { return new IntervalIntSet(this, from, to); }

        public override IntSet Ranges(IEnumerable<IntInterval> intervals) { return new IntervalIntSet(this, intervals); }

        public override MutableIntSet Mutable() { return new MutableIntervalIntSet(this); }
    }
}
