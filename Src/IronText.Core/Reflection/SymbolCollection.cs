using System.Collections.Generic;
using System.Linq;
using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class SymbolCollection : GrammarEntityCollection<Symbol, IGrammarScope>, ISymbolResolver
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
            if (symbolName == null)
            {
                return null;
            }

            var matcher = DR.Resolve<ISymbolTextMatcher>();

            var found = this.FirstOrDefault(s => matcher.Match(s, symbolName));
            if (found == null)
            {
                if (!createMissing)
                {
                    var msg = string.Format("Symbol name '{0}' not found.", symbolName);
                    throw new ArgumentException(msg, "symbolName");
                }

                found = Add(symbolName);
            }

            return found;
        }

        Symbol IReferenceResolver<Symbol,string>.Resolve(string name)
        {
            return ByName(name, true);
        }

        Symbol IReferenceResolver<Symbol, string>.Find(string name)
        {
            return ByName(name, false);
        }

        Symbol IReferenceResolver<Symbol, string>.Create(string name)
        {
            return Add(name);
        }
    }
}
