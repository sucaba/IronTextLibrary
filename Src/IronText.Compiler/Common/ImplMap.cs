using System;
using System.Collections.Generic;
using System.Linq;

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
            this.GetOrAdd(main, () => factory(main));

        public void EnsureMapped(T source)
        {
            Of(source);
        }

        public void EnsureMapped(T[] sources)
        {
            Array.ForEach(sources, EnsureMapped);
        }
    }

    class GraphImplMap<T, TImpl> : Dictionary<T, TImpl>
        where TImpl : new()
    {
        public IEnumerable<T>             Sources      => Keys;
        public IReadOnlyCollection<TImpl> Destinations => Values;

        private readonly Action<T, TImpl> initializer;
        private int initializationLock = 0;
        private List<KeyValuePair<T, TImpl>> initializationPlan = new List<KeyValuePair<T, TImpl>>();

        public GraphImplMap(Action<T,TImpl> initializer)
        {
            this.initializer = initializer;
        }

        public TImpl Of(T main)
        {
            try
            {
                return InternalOf(main);
            }
            finally
            {
                InitializePending();
            }
        }

        private TImpl InternalOf(T main)
        {
            TImpl result;

            if (!TryGetValue(main, out result))
            {
                result = new TImpl();
                Add(main, result);

                initializationPlan.Add(new KeyValuePair<T, TImpl>(main, result));
            }

            return result;
        }

        private void InitializePending()
        {
            if (initializationLock != 0)
            {
                return;
            }

            while (initializationPlan.Count != 0)
            {
                var pair = initializationPlan[0];

                ++initializationLock;
                try
                {
                    initializer(pair.Key, pair.Value);
                }
                finally
                {
                    --initializationLock;
                }

                initializationPlan.RemoveAt(0);
            }
        }

        private void GuardInitialization(Action action)
        {
            action();
        }

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
