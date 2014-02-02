using System;

namespace IronText.Collections
{
    public abstract class IndexableObject<TContext> : IIndexable<TContext>
    {
        public const int NoId = -1;

        public IndexableObject()
        {
            Index = NoId;
        }

        public int Index { get; private set; }

        public bool IsDetached { get { return NoId == Index; } }

        protected TContext Context { get; private set; }

        protected virtual void DoAttached() { }

        protected virtual void DoDetaching() { }

        void IIndexable<TContext>.Attach(int id, TContext context)
        {
            if (!IsDetached)
            {
                throw new InvalidOperationException("Object is already attached to a table.");
            }

            Index = id;
            Context = context;

            DoAttached();
        }

        void IIndexable<TContext>.Detach(TContext context)
        {
            DoDetaching();

            Index = NoId;
        }
    }
}
