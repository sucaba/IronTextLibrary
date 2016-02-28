namespace IronText.Runtime
{
    static class StackLookbackExtensions
    {
        public static IStackLookback<T> ShiftedLookback<T>(this IStackLookback<T> @this, int shift)
        {
            return new ShiftedStackLookbackDecorator<T>(@this, shift);
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
    }
}
