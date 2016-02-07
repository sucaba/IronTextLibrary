using System;
using System.Collections.Generic;

using Int = System.Int32;

namespace IronText.Algorithm
{
    /// <summary>
    /// Provides information on integer set type acts as a factory of the particular int set.
    /// </summary>
    [Serializable]
    public abstract class IntSetType
    {
        public abstract Int MinValue { get; }

        /// <summary>
        /// Valid max value. Used for complement operation.
        /// </summary>
        public abstract Int MaxValue { get; }

        public abstract IntSet All { get; }
        public abstract IntSet Empty { get; }

        public abstract IntSet Of(Int value);
        public abstract IntSet Of(IEnumerable<Int> value);
        public abstract IntSet Range(Int from, Int to);
        public abstract IntSet Ranges(IEnumerable<IntInterval> intervals);

        public abstract MutableIntSet Mutable();

        public IntSet Of(params Int[] values) { return Of((IEnumerable<Int>)values); }

        public IntSet Union(IEnumerable<IntSet> sets)
        {
            var result = Mutable();
            foreach (var set in sets)
            {
                result.AddAll(set);
            }

            return result.CompleteAndDestroy();
        }
    }
}
