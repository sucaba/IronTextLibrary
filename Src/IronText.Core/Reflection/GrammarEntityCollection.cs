using IronText.Algorithm;
using IronText.Collections;
using IronText.Misc;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class GrammarEntityCollection<T,TScope> : IndexedCollection<T, TScope>
        where T : class, IIndexable<TScope>, IHasIdentity
    {
        private BitSetType _indexSetType;

        public GrammarEntityCollection(TScope scope)
            : base(scope)
        {
        }

        protected override void WhenDoneBuildIndexes()
        {
            base.WhenDoneBuildIndexes();
            this._indexSetType = new BitSetType(Count);
        }

        public BitSetType IndexSetType
        {
            get
            {
                RequireIndexed();
                return _indexSetType;
            }
        }

        internal IDependencyResolver DR { get { return (IDependencyResolver)base.Scope; } }
    }
}
