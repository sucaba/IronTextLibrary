using System;

namespace IronText.Collections
{
    public struct AmbiguousAlternatives<T>
        where T : Ambiguous<T>
    {
        private readonly T first;

        public AmbiguousAlternatives(T first)
        {
            this.first = first;
        }

        public Enumerator GetEnumerator() =>
            new Enumerator(first);

        public T ResolveFirst(Func<T, bool> predicate)
        {
            foreach (var alternative in this)
            {
                if (predicate(alternative))
                {
                    return alternative;
                }
            }

            return null;
        }

        public class Enumerator
        {
            private readonly T first;
            private T next;

            public Enumerator(T current)
            {
                this.first   = current;
                this.Current = null;
                this.next    = current;
            }

            public T Current { get; private set; }

            public bool MoveNext()
            {
                Current = next;
                if (next != null)
                {
                    next = next.Alternative;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                Current = null;
                next    = first;
            }
        } 
    }
}
