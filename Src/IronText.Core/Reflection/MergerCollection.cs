using IronText.Collections;

namespace IronText.Reflection
{
    public class MergerCollection : IndexedCollection<Merger, ISharedGrammarEntities>
    {
        public MergerCollection(ISharedGrammarEntities ebnfContext)
            : base(ebnfContext)
        {
        }
    }
}
