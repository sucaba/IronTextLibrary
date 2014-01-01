using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanConditionCollection : IndexedCollection<Production,IEbnfContext>
    {
        public ScanConditionCollection(EbnfGrammar ebnfGrammar)
            : base(ebnfGrammar)
        {
        }
    }
}
