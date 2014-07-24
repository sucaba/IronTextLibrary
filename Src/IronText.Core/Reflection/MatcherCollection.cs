using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class MatcherCollection : IndexedCollection<Matcher,IGrammarScope>
    {
        public MatcherCollection(IGrammarScope context)
            : base(context)
        {
        }
    }
}
