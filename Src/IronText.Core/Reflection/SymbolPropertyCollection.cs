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
            if (dotExpression == null)
            {
                throw new ArgumentNullException("dotExpression");
            }

            string[] parts = dotExpression.Split('.');
            if (parts.Length != 2)
            {
                throw new InvalidOperationException("Invalid dot expression. Expected format is '<symbol-name>.<property>'.");
            }
            
            var result = new SymbolProperty(Scope.Symbols.ByName(parts[0].Trim(), createMissing: true), parts[1].Trim());
            Add(result);
            return result;
        }
    }
}
