using System;
using System.Collections;
using System.Collections.Generic;

namespace IronText.Collections
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
            Array.IndexOf(items, item) >= 0;

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
}
