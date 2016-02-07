using System;
using System.Collections.Generic;
using Int = System.Int32;

namespace IronText.Algorithm
{
    [Serializable]
    public sealed class IntervalIntSet : IntervalIntSetBase
    {
        /// <summary>
        /// Create empty set
        /// </summary>
        /// <param name="setType"></param>
        internal IntervalIntSet(IntSetType setType) : base(setType) { }

        internal IntervalIntSet(IntSetType setType, IEnumerable<Int> enumerated) : this(setType)
        {
            foreach (var item in enumerated)
            {
                Add(item);
            }

            UpdateHashAndBounds();
        }

        internal IntervalIntSet(IntSetType setType, Int first, Int last) : this(setType)
        {
            Add(new IntInterval(first, last));

            UpdateHashAndBounds();
        }

        internal IntervalIntSet(IntSetType setType, IEnumerable<IntInterval> intervals) : this(setType)
        {
            if (intervals == null)
            {
                throw new ArgumentNullException("intervals");
            }

            foreach (var interval in intervals)
            {
                Add(interval);
            }

            UpdateHashAndBounds();
        }

        public override IntSet CompleteAndDestroy()
        {
            throw new NotSupportedException();
        }
    }
}
