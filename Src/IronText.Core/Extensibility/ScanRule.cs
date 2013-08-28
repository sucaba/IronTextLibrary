using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronText.Extensibility
{
    internal abstract class ScanRule : IScanRule, IBootstrapScanRule
    {
        public MemberInfo DefiningMember { get; set; }

        public abstract IEnumerable<TokenRef[]> GetTokenRefGroups();

        bool IScanRule.IsSortable { get {  return LiteralText != null; } }

        public string Pattern { get; set; }

        public string LiteralText { get; set; }

        string IBootstrapScanRule.BootstrapRegexPattern { get { return BootstrapRegexPattern; } }

        internal string BootstrapRegexPattern { get; set; }

        public ScanActionBuilder ActionBuilder { get; set; }

        public Type NextModeType { get; set; }

        // for bootstrap
        internal bool ShouldSkip { get { return this is ISkipScanRule; } }

        int IScanRule.Priority { get; set; }

        // for sorting
        internal int Priority { get; set; }

        public static int ComparePriority(IScanRule x, IScanRule y)
        {
            return x.Priority - y.Priority;
        }

        public static bool IsMoreSpecialized(IScanRule x, IScanRule y)
        {
            var xRule = x as ISingleTokenScanRule;
            var yRule = y as ISingleTokenScanRule;
            if (yRule == null)
            {
                return xRule != null;
            }

            var xText = xRule.LiteralText;
            var yText = yRule.LiteralText;
            return xText != null && yText != null && xText.StartsWith(yText);
        }
    }

    internal class SkipScanRule : ScanRule, ISkipScanRule
    {
        public SkipScanRule()
        {

        }

        public override IEnumerable<TokenRef[]> GetTokenRefGroups()
        {
            return Enumerable.Empty<TokenRef[]>();
        }

        public override string ToString()
        {
            return string.Format("void -> {0}", Pattern);
        }
    }

    internal class SingleTokenScanRule : ScanRule, ISingleTokenScanRule
    {
        public SingleTokenScanRule()
        {

        }

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
