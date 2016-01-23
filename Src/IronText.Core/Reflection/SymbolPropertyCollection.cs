using IronText.Collections;
using System;
using System.Linq;

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

        public SymbolProperty Find(string symbolName, string property)
        {
            var result = this.FirstOrDefault(p => p.Symbol.Name == symbolName && p.Name == property);
            return result;
        }

        public SymbolProperty Find(Symbol symbol, string property)
        {
            var result = this.FirstOrDefault(p => symbol.Equals(p.Symbol) && p.Name == property);
            return result;
        }

        public SymbolProperty FindOrAdd(Symbol symbol, string property)
        {
            var result = Find(symbol, property) ?? Add(new SymbolProperty(symbol, property));
            return result;
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
