using System.Collections.Generic;
using System.Linq;
using IronText.Collections;
using System;

namespace IronText.Reflection
{
    public class SymbolCollection : IndexedCollection<SymbolBase, IGrammarScope>
    {
        public SymbolCollection(IGrammarScope context)
            : base(context)
        {
        }

        public Symbol Add(string name, SymbolCategory categories = SymbolCategory.None)
        {
            var result = new Symbol(name) { Categories = categories };
            Add(result);
            return result;
        }

        public AmbiguousSymbol FindOrAddAmbiguous(int mainToken, IEnumerable<int> tokens)
        {
            return FindAmbiguous(mainToken, tokens)
                ?? (AmbiguousSymbol)Add(new AmbiguousSymbol(mainToken, tokens));
        }

        public AmbiguousSymbol FindAmbiguous(int mainToken, IEnumerable<int> tokens)
        {
            foreach (var symb in Scope.Symbols)
            {
                var amb = symb as AmbiguousSymbol;
                if (amb != null && amb.MainToken == mainToken && amb.Tokens.SequenceEqual(tokens))
                {
                    return amb;
                }
            }

            return null;
        }

        public Symbol this[string symbolName]
        {
            get {  return ByName(symbolName, false); }
        }

        public Symbol ByName(string symbolName)
        {
            return ByName(symbolName, false);
        }

        public Symbol ByName(string symbolName, bool createMissing)
        {
            var found = this.FirstOrDefault(s => s.Name == symbolName);
            if (found == null)
            {
                if (!createMissing)
                {
                    var msg = string.Format("Symbol name '{0}' not found.", symbolName);
                    throw new ArgumentException(msg, "symbolName");
                }

                found = Add(symbolName);
            }
            
            var result = found as Symbol;
            if (result == null)
            {
                var msg = string.Format("Cannot search by symbol name '{0}'.", symbolName);
                throw new ArgumentException(msg, "symbolName");
            }

            return (Symbol)found;
        }
    }
}
