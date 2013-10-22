using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class SkipScanRule : ScanRule, ISkipScanRule
    {
        public SkipScanRule() { }

        public override TokenRef MainTokenRef { get { return null; } }

        public override IEnumerable<TokenRef[]> GetTokenRefGroups()
        {
            return Enumerable.Empty<TokenRef[]>();
        }

        public override string ToString()
        {
            return string.Format("void -> {0}", Pattern);
        }
    }
}