using System.Collections.Generic;
using System.Linq;
using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
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
