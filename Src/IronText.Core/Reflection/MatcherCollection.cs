using IronText.Collections;

namespace IronText.Reflection
{
    public class MatcherCollection : IndexedCollection<Matcher,ISharedGrammarEntities>
    {
        public MatcherCollection(ISharedGrammarEntities context)
            : base(context)
        {
        }
    }
}
