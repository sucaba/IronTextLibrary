using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal class SingleTokenScanRule : ScanRule, ISingleTokenScanRule
    {
        public SingleTokenScanRule() { }

        public TokenRef AnyTokenRef
        {
            get
            {
                if (LiteralText != null)
                {
                    return TokenRef.Literal(LiteralText);
                }

                if (TokenType != null && TokenType != typeof(object))
                {
                    return TokenRef.Typed(TokenType);
                }

                return null;
            }
        }

        public Type TokenType { get; set; }

        public override IEnumerable<TokenRef[]> GetTokenRefGroups()
        {
            yield return GetTokenGroup();
        }

        private TokenRef[] GetTokenGroup()
        {
            List<TokenRef> group = new List<TokenRef>();
            if (TokenType != null)
            {
                group.Add(TokenRef.Typed(TokenType));
            }

            if (LiteralText != null)
            {
                group.Add(TokenRef.Literal(LiteralText));
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
