using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ProductionCollection : IndexedCollection<Production, IGrammarScope>
    {
        public ProductionCollection(IGrammarScope scope)
            : base(scope)
        {
        }

        public Production Add(string outcome, IEnumerable<string> pattern, SemanticRef contextRef = null)
        {
            return Add(
                Scope.Symbols.ByName(outcome, createMissing: true),
                pattern.Select(name => Scope.Symbols.ByName(name, createMissing: true)),
                contextRef);
        }

        public Production Add(Symbol outcome, IEnumerable<Symbol> pattern, SemanticRef contextRef = null)
        {
            var result = new Production(outcome, pattern, contextRef);
            return Add(result);
        }

        [Obsolete("Refactoring grammar indexing")]
        public Production Add(int outcome, IEnumerable<int> pattern, string contextName = null)
        {
            return Add(
                (Symbol)Scope.Symbols[outcome],
                pattern.Select(t => (Symbol)Scope.Symbols[t]),
                SemanticRef.ByName(contextName));
        }

        public Production Find(string outcome, string[] pattern)
        {
            return Find(Scope.Symbols[outcome], Array.ConvertAll(pattern, Scope.Symbols.ByName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outcome"></param>
        /// <param name="pattern"></param>
        /// <returns>Rule ID or -1 if there is no such rule</returns>
        public Production Find(Symbol outcome, Symbol[] pattern)
        {
            int count = Count;
            for (int i = 0; i != count; ++i)
            {
                var prod = this[i];
                if (prod.Outcome == outcome
                    && prod.Pattern.Length == pattern.Length
                    && Enumerable.SequenceEqual(prod.Pattern, pattern)
                    && !prod.IsDeleted)
                {
                    return prod;
                }
            }

            return null;
        }
    }
}
