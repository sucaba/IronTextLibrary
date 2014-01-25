using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class CilSkipScanRule : CilScanRule, ICilSkipScanRule
    {
        public CilSkipScanRule() { }

        public override CilSymbolRef MainOutcome { get { return null; } set { }  }

        public override IEnumerable<CilSymbolRef> AllOutcomes
        {
            get { return Enumerable.Empty<CilSymbolRef>(); }
        }

        public override string ToString()
        {
            return string.Format("void -> {0}", Pattern);
        }
    }
}