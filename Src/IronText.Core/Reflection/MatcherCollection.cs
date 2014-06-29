using IronText.Collections;

namespace IronText.Reflection
{
    public class MatcherCollection : IndexedCollection<Matcher,IGrammarScope>
    {
        public MatcherCollection(IGrammarScope context)
            : base(context)
        {
        }
    }
}
