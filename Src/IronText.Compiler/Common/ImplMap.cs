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

        public IReadOnlyCollection<TImpl> Implementations => Values;

        public TImpl Of(T main) =>
            this.GetOrDefault(main, () => factory(main));
    }
}
