using System;
using System.Collections;
using System.Collections.Generic;

namespace IronText.Collections
{
    public struct AmbiguousAlternatives<T> : IEnumerable<T>
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

        public T SingleOrDefault()
        {
            if (first == null || first.Alternative != null)
            {
                return default(T);
            }

            return first;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        public class Enumerator : IEnumerator<T>
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

            object IEnumerator.Current => Current;

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

            public void Dispose()
            {
            }
        } 
    }
}
