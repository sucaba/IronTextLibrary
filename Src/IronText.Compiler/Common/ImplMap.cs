using System;
using System.Collections.Generic;

namespace IronText.Common
{
    class ImplMap<T, TImpl> : Dictionary<T, TImpl>
    {
        private readonly Func<T, TImpl> factory;

        public ImplMap(Func<T,TImpl> factory)
        {
            this.factory = factory;
        }

        public IEnumerable<T> Sources => Keys;

        public IReadOnlyCollection<TImpl> Destinations => Values;

        public TImpl Of(T main) =>
            this.GetOrDefault(main, () => factory(main));

        public void EnsureMapped(T source)
        {
            Of(source);
        }

        public void EnsureMapped(T[] sources)
        {
            Array.ForEach(sources, EnsureMapped);
        }
    }
}
