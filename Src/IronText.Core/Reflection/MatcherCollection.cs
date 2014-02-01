using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class MatcherCollection : IndexedCollection<Matcher,IEbnfEntities>
    {
        public MatcherCollection(IEbnfEntities context)
            : base(context)
        {
        }
    }
}
