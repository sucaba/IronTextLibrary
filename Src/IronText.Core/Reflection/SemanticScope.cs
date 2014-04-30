using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;
using System.Collections.ObjectModel;

namespace IronText.Reflection
{
    public class SemanticScope : Collection<SemanticValue>
    {
        public SemanticScope()
        {
            this.Joint = new Joint();
        }

        public bool Provides(SemanticRef reference)
        {
            return this.Any(c => c.Match(reference));
        }

        public SemanticValue Resolve(SemanticRef reference)
        {
            var matching = this.Where(c => c.Match(reference));
            int count = matching.Count();
            switch (count)
            {
                case 0: return null;
                case 1: return matching.First();
                default:
                    throw new InvalidOperationException(
                        "Ambiguous context reference: " + reference.UniqueName);
            }
        }

        public Joint Joint { get; private set; }

        public bool Lookup(string name)
        {
            return this.Any(obj => obj.UniqueName == name);
        }
    }
}
