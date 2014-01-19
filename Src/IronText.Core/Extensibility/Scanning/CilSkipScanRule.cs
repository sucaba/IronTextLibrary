using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class CilSkipScanRule : CilScanRule, ICilSkipScanRule
    {
        public CilSkipScanRule() { }

        public override CilSymbolRef MainTokenRef { get { return null; } }

        public override IEnumerable<CilSymbolRef[]> GetTokenRefGroups()
        {
            return Enumerable.Empty<CilSymbolRef[]>();
        }

        public override string ToString()
        {
            return string.Format("void -> {0}", Pattern);
        }
    }
}