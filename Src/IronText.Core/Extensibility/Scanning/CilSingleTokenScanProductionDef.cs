using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class CilSingleTokenScanRule : CilScanRule, ICilSingleTokenScanRule
    {
        public override CilSymbolRef MainTokenRef { get { return GetTokenGroup().FirstOrDefault(); } }

        public CilSymbolRef AnyTokenRef { get { return GetTokenGroup().FirstOrDefault(); } }

        public override IEnumerable<CilSymbolRef[]> GetTokenRefGroups()
        {
            return new [] { GetTokenGroup().ToArray() };
        }

        private IEnumerable<CilSymbolRef> GetTokenGroup()
        {
            if (SymbolTypes.Count != 0)
            {
                yield return CilSymbolRef.Typed(SymbolTypes[0]);
            }

            if (LiteralText != null)
            {
                yield return CilSymbolRef.Literal(LiteralText);
            }
        }

        public override string ToString()
        {
            var group = GetTokenGroup().ToArray();
            string leftSide;
            if (group.Length > 1)
            {
                leftSide = string.Format("{{{0}}}", string.Join<string>(" | ", group.Select(tid => tid.Name)));
            }
            else
            {
                leftSide = group[0].Name;
            }

            return string.Format("{0} -> {1}", leftSide, Pattern);
        }
    }
}
