using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class ForeignContextProvider : ReferenceCollection<ForeignContext>
    {
        public ForeignContextProvider()
        {
            this.Joint = new Joint();
        }

        public bool Provides(ForeignContextRef reference)
        {
            return this.Any(c => c.Match(reference));
        }

        public ForeignContext Resolve(ForeignContextRef reference)
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
