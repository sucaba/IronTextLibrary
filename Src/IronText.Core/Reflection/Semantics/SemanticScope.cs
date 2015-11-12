using IronText.Collections;
using System;
using System.Collections.Generic;

namespace IronText.Reflection
{
    [Serializable]
    public class SemanticScope : IEnumerable<KeyValuePair<SemanticRef,SemanticValue>>
    {
        private Dictionary<SemanticRef, SemanticValue> dictionary = new Dictionary<SemanticRef, SemanticValue>();

        [NonSerialized]
        private readonly Joint _joint = new Joint();

        public Joint Joint { get { return _joint; } }

        public bool Lookup(SemanticRef reference)
        {
            return dictionary.ContainsKey(reference);
        }

        public SemanticValue Resolve(SemanticRef reference)
        {
            SemanticValue result;
            return dictionary.TryGetValue(reference, out result) ? result : null;
        }

        public void Add(SemanticRef reference, SemanticValue context)
        {
            dictionary.Add(reference, context);
        }

        IEnumerator<KeyValuePair<SemanticRef, SemanticValue>> IEnumerable<KeyValuePair<SemanticRef, SemanticValue>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}
