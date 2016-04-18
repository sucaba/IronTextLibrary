using IronText.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime.Inlining
{
    class FixedSizeStack<T>
    {
        private readonly T[] items;
        private int count;

        public FixedSizeStack(int size)
        {
            this.items = new T[size];
            this.count = 0;
        }

        public T Peek() => items[count - 1];

        public void Pop(int count)
        {
            this.count -= count;
        }

        public void Push(T item)
        {
            items[count++] = item;
        }

        public ArraySlice<T> GetArraySlice(int count)
            => new ArraySlice<T>(items, this.count - count, count);

        public IStackLookback<T> GetLookback(int parentState)
            => items.ToLookback(count, parentState);
    }
}
