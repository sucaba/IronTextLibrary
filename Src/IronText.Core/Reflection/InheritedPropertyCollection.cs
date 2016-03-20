using IronText.Collections;
using System;
using System.Linq;

namespace IronText.Reflection
{
    [Serializable]
    public class InheritedPropertyCollection : IndexedCollection<InheritedProperty, IGrammarScope>
    {
        public InheritedPropertyCollection(IGrammarScope scope)
            : base(scope)
        {
        }

        public void Add(string symbolName, string property)
        {
            if (null == Find(symbolName, property))
            {
                Add(new InheritedProperty(Scope.Symbols[symbolName], property));
            }
        }

        public InheritedProperty Find(string symbolName, string property)
        {
            var result = this.FirstOrDefault(p => p.Symbol.Name == symbolName && p.Name == property);
            return result;
        }

        public InheritedProperty Find(Symbol symbol, string property)
        {
            var result = this.FirstOrDefault(p => p.Symbol.Equals(symbol) && p.Name == property);
            return result;
        }

        public InheritedProperty FindOrAdd(Symbol symbol, string property)
        {
            var result = Find(symbol, property) ?? Add(new InheritedProperty(symbol, property));
            return result;
        }

        public InheritedProperty Add(string dotExpression)
        {
            string[] parts = DotExpression.Parse(dotExpression);
            
            var result = new InheritedProperty(Scope.Symbols.ByName(parts[0], createMissing: true), parts[1]);
            Add(result);
            return result;
        }

        internal InheritedProperty Resolve(Production prod, SemanticReference semanticReference)
        {
            if (semanticReference.Position >= 0)
            {
                throw new InvalidOperationException("INH SemanticReferecene should refer left side only.");
            }

            Symbol symbol = prod.Outcome;
            string name   = semanticReference.Name;

            return Find(symbol, name);
        }

        internal InheritedProperty Resolve(Production prod, SemanticVariable var)
        {
            if (var.Position < 0)
            {
                throw new InvalidOperationException("INH SemanticValue should refer right side only.");
            }

            Symbol symbol = prod.Input[var.Position];
            string name   = var.Name;

            return Find(symbol, name);
        }
    }
}
