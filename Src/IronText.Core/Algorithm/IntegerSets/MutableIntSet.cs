using System;
using Int = System.Int32;

namespace IronText.Algorithm
{
    [Serializable]
    public abstract class MutableIntSet : IntSet
    {
        protected MutableIntSet(IntSetType setType) : base(setType) { }

        public abstract bool Add(Int value);
        public abstract int Add(IntInterval interval);
        public abstract void Remove(Int value);
        public abstract int AddAll(IntSet other);
        public abstract void RemoveAll(IntSet other);

        /// <summary>
        /// Removes and returns any element from set
        /// </summary>
        /// <returns>Removed element of a set</returns>
        /// <exception cref="InvalidOperationException">
        /// when set is empty
        /// </exception>
        public abstract Int PopAny();

        public abstract IntSet CompleteAndDestroy();
    }
}
