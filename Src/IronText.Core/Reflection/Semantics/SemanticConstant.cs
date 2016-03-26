using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    [Serializable]
    public class SemanticConstant : ISemanticValue
    {
        public SemanticConstant(object value)
        {
            this.Value = value;
        }

        public object Value { get; set; }
    }
}
