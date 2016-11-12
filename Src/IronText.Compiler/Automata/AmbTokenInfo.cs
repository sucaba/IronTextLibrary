using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata
{
    internal class AmbTokenInfo
    {
        public AmbTokenInfo(int envelopeIndex, IEnumerable<int> tokens)
        {
            this.EnvelopeIndex = envelopeIndex;
            this.Alternatives  = tokens.ToArray();
        }

        public int   EnvelopeIndex { get; private set; }

        public int[] Alternatives  { get; private set; }
    }
}
