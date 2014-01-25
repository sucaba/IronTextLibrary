using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class CilSingleTokenScanRule : CilScanRule, ICilSingleTokenScanRule
    {
        public override CilSymbolRef MainOutcome { get { return GetTokenGroup(); } }

        public CilSymbolRef AnyTokenRef { get { return GetTokenGroup(); } }

        public override IEnumerable<CilSymbolRef> GetAllOutcomes()
        {
            return new [] { GetTokenGroup() };
        }

        private CilSymbolRef GetTokenGroup()
        {
            if (SymbolTypes.Count != 0 || LiteralText != null)
            {
                return new CilSymbolRef(SymbolTypes.FirstOrDefault(), LiteralText);
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
