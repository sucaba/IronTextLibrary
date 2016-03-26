using System;

namespace IronText.Runtime.Semantics
{
    [Serializable]
    public class RuntimeConstant : IRuntimeValue
    {
        private readonly object value;

        public RuntimeConstant(object value)
        {
            this.value = value;
        }

        public object Eval(IStackLookback<ActionNode> lookback)
        {
            return value;
        }
    }
}
