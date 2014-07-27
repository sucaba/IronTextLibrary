using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    [Serializable]
    public class AmbiguousSymbolCollection : Collection<AmbiguousSymbol>
    {
        private readonly IGrammarScope scope;

        public AmbiguousSymbolCollection(IGrammarScope scope)
        {
            this.scope = scope;
        }

        public AmbiguousSymbol FindOrAdd(int mainToken, IEnumerable<int> tokens)
        {
            var result = Find(mainToken, tokens);
            if (result == null)
            {
                result = new AmbiguousSymbol(mainToken, tokens);
                Add(result);
            }
            
            return result;
        }
    
        public AmbiguousSymbol Find(int mainToken, IEnumerable<int> tokens)
        {
            foreach (var symb in scope.Symbols)
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
