using IronText.Collections;

namespace IronText.Reflection
{
    public class MergerCollection : IndexedCollection<Merger, IGrammarScope>
    {
        public MergerCollection(IGrammarScope ebnfContext)
            : base(ebnfContext)
        {
        }
    }
}
