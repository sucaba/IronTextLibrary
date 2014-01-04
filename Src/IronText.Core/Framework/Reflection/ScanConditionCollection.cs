using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanConditionCollection : IndexedCollection<ScanCondition,IEbnfContext>
    {
        public ScanConditionCollection(EbnfGrammar ebnfGrammar)
            : base(ebnfGrammar)
        {
        }
    }
}
