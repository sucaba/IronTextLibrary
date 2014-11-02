using IronText.Collections;
using IronText.Misc;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class GrammarEntityCollection<T,TScope> : IndexedCollection<T, TScope>
        where T : class, IIndexable<TScope>, IHasIdentity
    {
        public GrammarEntityCollection(TScope scope)
            : base(scope)
        {
        }

        internal IDependencyResolver DR { get { return (IDependencyResolver)base.Scope; } }
    }
}
