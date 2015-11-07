using IronText.Misc;
using System;

namespace IronText.Collections
{
    [Serializable]
    public abstract class IndexableObject<TScope> 
        : IIndexable<TScope>, IHasIdentity
        where TScope : class
    {
        private const int NoId = -1;

        private bool hasIndex;
        private int _index = NoId;

        public IndexableObject()
        {
        }

        internal int Index 
        {
            get 
            {  
                if (!hasIndex)
                {
                    throw new InvalidOperationException("Object was not indexed.");
                }

                return this._index; 
            }
            set
            {
                this._index    = value;
                this.hasIndex  = true;
            }
        }

        public bool IsDetached { get { return Scope == null; } }

        public bool IsSoftRemoved  { get; private set; }

        protected TScope Scope { get; private set; }

        public void SoftRemove()
        {
            OnDetaching();
            this.IsSoftRemoved = true;
        }

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

            this._index = NoId;
            hasIndex = false;
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
