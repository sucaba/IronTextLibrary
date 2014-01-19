using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class CilSingleTokenScanRule : CilScanRule, ICilSingleTokenScanRule
    {
        public CilSingleTokenScanRule() { }

        public override CilSymbolRef MainTokenRef { get { return AnyTokenRef; } }

        public CilSymbolRef AnyTokenRef
        {
            get
            {
                if (LiteralText != null)
                {
                    return CilSymbolRef.Literal(LiteralText);
                }

                if (TokenType != null && TokenType != typeof(object))
                {
                    return CilSymbolRef.Typed(TokenType);
                }

                return null;
            }
        }

        public Type TokenType { get; set; }

        public override IEnumerable<CilSymbolRef[]> GetTokenRefGroups()
        {
            yield return GetTokenGroup();
        }

        private CilSymbolRef[] GetTokenGroup()
        {
            List<CilSymbolRef> group = new List<CilSymbolRef>();
            if (TokenType != null)
            {
                group.Add(CilSymbolRef.Typed(TokenType));
            }

            if (LiteralText != null)
            {
                group.Add(CilSymbolRef.Literal(LiteralText));
            }

            return group.ToArray();
        }

        public override string ToString()
        {
            var group = GetTokenGroup();
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
