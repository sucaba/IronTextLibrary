using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Compiler.Analysis
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
