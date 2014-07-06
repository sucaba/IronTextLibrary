using IronText.Misc;
using System;

namespace IronText.Collections
{
    public abstract class IndexableObject<TScope> : IIndexable<TScope>, IHasIdentity
    {
        public const int NoId = -1;

        public IndexableObject()
        {
            Index = NoId;
        }

        internal int Index { get; private set; }

        public bool IsDetached { get { return NoId == Index; } }

        protected TScope Scope { get; private set; }

        protected virtual void DoAttached() { }

        protected virtual void DoDetaching() { }

        void IIndexable<TScope>.Attached(int id, TScope context)
        {
            if (!IsDetached)
            {
                throw new InvalidOperationException("Object is already attached to a table.");
            }

            Index = id;
            Scope = context;

            DoAttached();
        }

        void IIndexable<TScope>.Detaching(TScope context)
        {
            DoDetaching();

            Index = NoId;
        }

        object IHasIdentity.Identity
        {
            get { return DoGetIdentity(); }
        }

        protected virtual object DoGetIdentity()
        {
            return IdentityFactory.FromObject(this);
        }
    }
}
