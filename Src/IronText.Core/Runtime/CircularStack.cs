using System;

namespace IronText.Framework
{
    internal sealed class CircularStack<T>
    {
        private readonly T[] items;
        private int pos;
        private int capacity;

        public CircularStack(int size)
        {
            this.capacity = size;
            this.items = new T[size];
        }

        public void Push(T item)
        {
            items[pos] = item;
            pos = (pos + 1) % capacity;
        }

        public T Pop()
        {
            pos = (pos + capacity - 1) % capacity;
            T result = items[pos];
            items[pos] = default(T);
            return result;
        }

        public void Clear()
        {
            Array.Clear(items, 0, items.Length);
            pos = 0;
        }
    }
}
