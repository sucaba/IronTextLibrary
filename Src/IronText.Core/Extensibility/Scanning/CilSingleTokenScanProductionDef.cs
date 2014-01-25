using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class CilSingleTokenScanRule : CilScanRule, ICilSingleTokenScanRule
    {
        public override CilSymbolRef MainOutcome { get { return GetTokenGroup(); } set { }  }

        public override IEnumerable<CilSymbolRef> AllOutcomes
        {
            get { return new[] { GetTokenGroup() }; }
        }

        private CilSymbolRef GetTokenGroup()
        {
            if (SymbolType != null || LiteralText != null)
            {
                return new CilSymbolRef(SymbolType, LiteralText);
            }

            throw new InvalidOperationException();
        }

        public override string ToString()
        {
            var leftSide = GetTokenGroup().ToString();
            return string.Format("{0} -> {1}", leftSide, Pattern);
        }
    }
}
