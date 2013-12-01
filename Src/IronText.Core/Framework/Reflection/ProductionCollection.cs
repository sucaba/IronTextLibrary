using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionCollection : IndexedCollection<Production, IEbnfContext>
    {
        public ProductionCollection(IEbnfContext context)
            : base(context)
        {
        }

        public Production Add(Symbol outcome, IEnumerable<Symbol> pattern)
        {
            var result = new Production(outcome, pattern);
            return Add(result);
        }

        [Obsolete("Refactoring grammar indexing")]
        public Production Add(int outcome, IEnumerable<int> pattern)
        {
            return Add(
                (Symbol)Context.Symbols[outcome],
                pattern.Select(t => (Symbol)Context.Symbols[t]));
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
                if (prod.OutcomeSymbol == outcome
                    && prod.PatternSymbols.Length == pattern.Length
                    && Enumerable.SequenceEqual(prod.PatternSymbols, pattern))
                {
                    return prod;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outcome"></param>
        /// <param name="pattern"></param>
        /// <param name="?"></param>
        /// <returns><c>true</c> when production was just defined, <c>false</c> if it existed previously</returns>
        public bool FindOrAdd(Symbol outcome, Symbol[] pattern, out Production output)
        {
            output = Find(outcome, pattern);

            if (output == null)
            {
                output = Add(outcome, pattern);
                return true;
            }

            return false;
        }
    }
}
