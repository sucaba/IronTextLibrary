using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;
using System.Collections.ObjectModel;

namespace IronText.Reflection
{
    public class SemanticScope
    {
        private Dictionary<SemanticRef, SemanticValue> dictionary = new Dictionary<SemanticRef, SemanticValue>();

        public SemanticScope()
        {
            this.Joint = new Joint();
        }

        public Joint Joint { get; private set; }

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
    }
}
