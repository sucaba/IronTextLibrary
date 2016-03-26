using System;

namespace IronText.Runtime.Semantics
{
    [Serializable]
    public struct InheritedRuntimeProperty : IRuntimeValue, IRuntimeVariable
    {
        public InheritedRuntimeProperty(int offset, int index)
        {
            this.Offset = offset;
            this.Index  = index;
        }

        public int Offset { get; private set; }

        public int Index  { get; private set; }

        public object Eval(IStackLookback<ActionNode> lookback)
        {
            var node = lookback.GetNodeAt(Offset);
            var result = node.GetInheritedStateProperty(Index);
            return result;
        }

        public void Assign(IStackLookback<ActionNode> lookback, object value)
        {
            var node = lookback.GetNodeAt(Offset);
            node.SetInheritedStateProperty(Index, value);
        }
    }
}
