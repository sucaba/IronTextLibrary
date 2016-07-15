using System;

namespace IronText.Runtime
{
    static class StackLookbackExtensions
    {
        public static IStackLookback<T> Pushed<T>(this IStackLookback<T> @this, T[] items)
        {
            return new PushedStackLookback<T>(@this, items);
        }

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

        class PushedStackLookback<T> : IStackLookback<T>
        {
            private readonly IStackLookback<T> original;
            private readonly T[]               items;

            public PushedStackLookback(
                IStackLookback<T> original,
                T[]               items)
            {
                this.original = original;
                this.items    = items;
            }

            public T GetNodeAt(int backOffset)
            {
                T result;

                int index = items.Length - backOffset;
                if (index >= 0)
                {
                    result = items[index];
                }
                else
                {
                    result = original.GetNodeAt(-index);
                }

                return result;
            }

            public int GetState(int backOffset)
            {
                int index = items.Length - backOffset;
                if (index >= 0)
                {
                    throw new ArgumentException(nameof(backOffset));
                }

                int result = original.GetState(-index);
                return result;
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
