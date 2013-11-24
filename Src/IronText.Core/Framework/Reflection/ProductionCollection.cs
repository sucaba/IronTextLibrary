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

        public Production Add(int outcome, int[] pattern)
        {
            var result = new Production { Outcome = outcome, Pattern = pattern };
            return Add(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outcome"></param>
        /// <param name="pattern"></param>
        /// <returns>Rule ID or -1 if there is no such rule</returns>
        public Production Find(int outcome, int[] pattern)
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
        public bool FindOrAdd(int outcome, int[] pattern, out Production output)
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
