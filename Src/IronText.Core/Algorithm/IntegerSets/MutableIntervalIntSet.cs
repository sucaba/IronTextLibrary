using System.Collections.Generic;

namespace IronText.Algorithm
{
    sealed internal class MutableIntervalIntSet : IntervalIntSetBase
    {
        public MutableIntervalIntSet(IntSetType setType) : base(setType) { }

        public override IntSet CompleteAndDestroy()
        {
            var result = new IntervalIntSet(setType, this.intervals);
            this.intervals = new List<IntInterval>();
            UpdateHashAndBounds();
            return result;
        }

        public override MutableIntSet EditCopy()
        {
            var result = new MutableIntervalIntSet(setType);
            result.AddAll(this);
            return result;
        }
    }
}
