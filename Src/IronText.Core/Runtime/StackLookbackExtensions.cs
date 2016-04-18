using System;

namespace IronText.Runtime
{
    static class StackLookbackExtensions
    {
        public static IStackLookback<T> ShiftedLookback<T>(this IStackLookback<T> @this, int shift)
        {
            return new ShiftedStackLookbackDecorator<T>(@this, shift);
        }

        public static IStackLookback<T> ToLookback<T>(this T[] items, int index, int parentState)
        {
            return new ArrayLookback<T>(items, index, parentState);
        }

        class ShiftedStackLookbackDecorator<T> : IStackLookback<T>
        {
            private readonly IStackLookback<T> original;
            private readonly int               shiftCount;

            public ShiftedStackLookbackDecorator(
                IStackLookback<T> original,
                int               shiftCount)
            {
                this.original   = original;
                this.shiftCount = shiftCount;
            }

            public T GetNodeAt(int backOffset)
            {
                return original.GetNodeAt(backOffset + shiftCount);
            }

            public int GetParentState()
            {
                return GetState(1);
            }

            public int GetState(int backOffset)
            {
                return original.GetState(shiftCount + backOffset);
            }
        }

        struct ArrayLookback<T> : IStackLookback<T>
        {
            private readonly int index;
            private readonly T[] items;
            private readonly int parentState;

            public ArrayLookback(T[] items, int index, int parentState)
            {
                this.items = items;
                this.index = index;
                this.parentState = parentState;
            }

            public T GetNodeAt(int backOffset) => items[index - backOffset]; 

            public int GetParentState() => parentState;

            public int GetState(int backOffset)
            {
                throw new NotImplementedException();
            }
        }
    }
}
