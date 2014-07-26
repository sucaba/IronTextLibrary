using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Compiler.Analysis
{
    internal class AmbTokenInfo
    {
        public AmbTokenInfo(int index, int mainToken, IEnumerable<int> allTokens)
        {
            this.Index     = index;
            this.MainToken = mainToken;
            this.AllTokens = allTokens.ToArray();
        }

        public int   Index     { get; private set; }

        public int   MainToken { get; private set; }

        public int[] AllTokens { get; private set; }
    }
}
