using System;

namespace IronText.Runtime
{
    static class StackLookbackExtensions
    {
        public static IStackLookback<T> ToLookback<T>(this T[] items, int index)
        {
            return new ArrayLookback<T>(items, index);
        }

        public static void CopyTo<T>(this IStackLookback<T> @this, T[] array, int count)
        {
            for (int i = 0; i != count; ++i)
            {
                array[i] = @this.GetNodeAt(count - i);
            }
        }

        struct ArrayLookback<T> : IStackLookback<T>
        {
            private readonly int index;
            private readonly T[] items;

            public ArrayLookback(T[] items, int index)
            {
                this.items = items;
                this.index = index;
            }

            public T GetNodeAt(int backOffset) => items[index - backOffset];

            public int GetState(int backOffset)
            {
                throw new NotImplementedException();
            }
        }
    }
}
