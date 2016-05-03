using System.Collections.Generic;

namespace IronText.Runtime
{
    class GssReductionIndex<T>
    {
        private readonly Dictionary<long,T> keyToValue = new Dictionary<long,T>();

        public T this[int token, int startLayer]
        {
            get { return keyToValue[GetNKey(token, startLayer)]; }
            set { keyToValue[GetNKey(token, startLayer)] = value; }
        }

        public void Clear() => keyToValue.Clear();

        public bool TryGet(int token, int startLayer, out T outcome)
            =>
            keyToValue.TryGetValue(GetNKey(token, startLayer), out outcome);

        private static long GetNKey(long X, int startLayer)
            => (X << 32) + startLayer;
    }
}
