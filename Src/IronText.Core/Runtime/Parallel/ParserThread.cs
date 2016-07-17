using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    class ParserThread<T> : IStackLookback<T>
    {
        public ParserThread(int state, T value, ParserThread<T> previous = null)
        {
            State = state;
            Value = value;
            Previous = previous;
        }

        public ParserThread<T> Previous { get; }

        public int  State { get; }

        public T    Value { get; }

        T IStackLookback<T>.GetNodeAt(int backOffset) =>
            GetAtDepth(backOffset).Value;

        int IStackLookback<T>.GetState(int backOffset) =>
            GetAtDepth(backOffset).State;

        private ParserThread<T> GetAtDepth(int backOffset)
        {
            var node = this;
            while (0 != --backOffset)
            {
                node = node.Previous;
            }

            return node;
        }

        public ParserThread<T> Clone() =>
            new ParserThread<T>(
                State,
                Value,
                Previous);
    }
}
