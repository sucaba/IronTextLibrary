using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class SematnicContextProvider : ReferenceCollection<SemanticContext>
    {
        public SematnicContextProvider()
        {
            this.Joint = new Joint();
        }

        public bool Provides(SemanticContextRef reference)
        {
            return this.Any(c => c.Match(reference));
        }

        public SemanticContext Resolve(SemanticContextRef reference)
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
    }
}
