using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    public class ImmutableArray<T> : IEnumerable<T>
    {
        protected readonly T[] items;
        protected int count;

        public ImmutableArray(int capacity)
        {
            this.items = new T[capacity];
        }

        public ImmutableArray(T[] items, int count)
        {
            this.items = items;
            this.count = count;
        }

        public T this[int index] => items[index];

        public int Capacity => items.Length;

        public bool IsEmpty => count == 0;

        public int Count => count;

        public bool Contains(T item) =>
            Array.IndexOf(items, item) > 0;

        public Enumerator GetEnumerator()
            => new Enumerator(items, count);

        public class Enumerator
            : IEnumerator<T>
            , IEnumerator
        {
            private readonly T[] items;
            private readonly int count;
            private int index;

            public Enumerator(T[] items, int count)
            {
                this.items = items;
                this.index = -1;
                this.count = count;
            }

            public T Current => items[index];

            object IEnumerator.Current => Current;

            public bool MoveNext() => ++index != count;

            public void Reset() => index = -1;

            public void Dispose() { }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

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
