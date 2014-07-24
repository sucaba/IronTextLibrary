using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class MergerCollection : IndexedCollection<Merger, IGrammarScope>
    {
        public MergerCollection(IGrammarScope ebnfContext)
            : base(ebnfContext)
        {
        }
    }
}
