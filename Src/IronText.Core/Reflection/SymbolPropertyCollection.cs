using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    /// <summary>
    /// Collection of symbol properties (AKA Synthesized Attribute).
    /// </summary>
    [Serializable]
    public class SymbolPropertyCollection : IndexedCollection<SymbolProperty,IGrammarScope>
    {
        public SymbolPropertyCollection(IGrammarScope scope)
            : base(scope)
        {
        }

        /// <summary>
        /// Add symbol peroperty by dot-expression.
        /// </summary>
        /// <param name="dotExpression">Expression in format 'SYMBOL.PROPERTY'.</param>
        /// <returns><see cref="SymbolProperty"/> instance</returns>
        public SymbolProperty Add(string dotExpression)
        {
            string[] parts = DotExpression.Parse(dotExpression);
            
            var result = new SymbolProperty(Scope.Symbols.ByName(parts[0], createMissing: true), parts[1]);
            Add(result);
            return result;
        }
    }
}
