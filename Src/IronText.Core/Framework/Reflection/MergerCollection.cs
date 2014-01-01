using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class MergerCollection : IndexedCollection<Merger, IEbnfContext>
    {
        public MergerCollection(EbnfGrammar ebnfGrammar)
            : base(ebnfGrammar)
        {
        }
    }
}
