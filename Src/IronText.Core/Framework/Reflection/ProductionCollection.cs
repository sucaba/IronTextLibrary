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
    }
}
