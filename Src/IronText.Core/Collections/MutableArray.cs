using System;

namespace IronText.Collections
{
    public class MutableArray<T>
        : ImmutableArray<T>
        , ICloneable
    {
        public MutableArray(int capacity)
            : base(capacity)
        {
        }

        private MutableArray(T[] items, int count)
            : base(items, count)
        {
        }

        public void AddDistinct(T item)
        {
            if (!Contains(item))
            {
                Add(item);
            }
        }

        public void Add(T item) =>
            items[count++] = item;
        
        public void Clear() => count = 0;

        public void RemoveAll(Func<T, bool> criteria)
        {
            int srcIndex = 0, destIndex = 0;
            while (srcIndex != count)
            {
                var src = items[srcIndex];
                if (!criteria(src))
                {
                    items[destIndex++] = src;
                }

                ++srcIndex;
            }

            count = destIndex;
        }

        public MutableArray<T> Clone()
        {
            var resultItems = new T[Capacity];
            Array.Copy(items, resultItems, count);
            return new MutableArray<T>(resultItems, count);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}