using IronText.Misc;
using System;

namespace IronText.Collections
{
    [Serializable]
    public abstract class IndexableObject<TScope> : IIndexable<TScope>, IHasIdentity
    {
        public const int NoId = -1;

        public IndexableObject()
        {
            Index = NoId;
        }

        internal int Index { get; private set; }

        public bool IsDetached { get { return NoId == Index; } }

        public bool IsHidden  { get; private set; }

        protected TScope Scope { get; private set; }

        public void Hide()
        {
            OnHiding();
            this.IsHidden = true;
            OnHided();
        }

        protected virtual void OnHiding() { }

        protected virtual void OnHided() { }

        protected virtual void OnAttached() { }

        protected virtual void OnDetaching() { }

        void IIndexable<TScope>.Attached(TScope context)
        {
            if (!IsDetached)
            {
                throw new InvalidOperationException("Object is already attached to a table.");
            }

            Scope = context;

            OnAttached();
        }

        void IIndexable<TScope>.AssignIndex(int index)
        {
            this.Index = index;
        }

        void IIndexable<TScope>.Detaching(TScope context)
        {
            OnDetaching();

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
