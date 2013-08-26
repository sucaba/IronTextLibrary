using Int = System.Int32;

namespace IronText.Algorithm
{
    public abstract class MutableIntSet : IntSet
    {
        protected MutableIntSet(IntSetType setType) : base(setType) { }

        public abstract void Add(Int value);
        public abstract void Add(IntInterval interval);
        public abstract void Remove(Int value);
        public abstract void AddAll(IntSet other);
        // TODO:
        // public abstract bool RemoveAll(IntSet other);

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
