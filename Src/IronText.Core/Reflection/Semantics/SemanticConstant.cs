using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Runtime.Semantics;

namespace IronText.Reflection
{
    [Serializable]
    public class SemanticConstant : ISemanticValue
    {
        public SemanticConstant(object value)
        {
            this.Value = value;
        }

        public object Value { get; private set; }

        public IRuntimeValue ToRuntime(int _)
        {
            return new RuntimeConstant(Value);
        }
    }
}
