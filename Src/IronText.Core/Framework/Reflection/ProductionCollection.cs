using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    public class ProductionCollection : IndexedCollection<Production, IEbnfContext>
    {
        public ProductionCollection(IEbnfContext context)
            : base(context)
        {
        }

        public Production Define(Symbol outcome, IEnumerable<Symbol> pattern)
        {
            var result = new Production(outcome, pattern);
            return Add(result);
        }

        [Obsolete("Refactoring grammar indexing")]
        public Production Define(int outcome, IEnumerable<int> pattern)
        {
            return Define(
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
                if (prod.Outcome == outcome
                    && prod.Pattern.Length == pattern.Length
                    && Enumerable.SequenceEqual(prod.Pattern, pattern))
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
                output = Define(outcome, pattern);
                return true;
            }

            return false;
        }
    }
}
