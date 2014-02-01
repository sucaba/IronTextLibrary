using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ConditionCollection : IndexedCollection<Condition,ISharedGrammarEntities>
    {
        public ConditionCollection(Grammar ebnfGrammar)
            : base(ebnfGrammar)
        {
        }
    }
}
