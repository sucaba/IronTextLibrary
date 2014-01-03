using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class SymbolCollection : IndexedCollection<SymbolBase, IEbnfContext>
    {
        public SymbolCollection(IEbnfContext context)
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
            foreach (var symb in Context.Symbols)
            {
                var amb = symb as AmbiguousSymbol;
                if (amb != null && amb.MainToken == mainToken && amb.Tokens.SequenceEqual(tokens))
                {
                    return amb;
                }
            }

            return null;
        }
    }
}
