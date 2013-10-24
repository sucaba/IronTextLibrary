using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    internal abstract class ScanRule : IScanRule, IBootstrapScanRule
    {
        public MemberInfo DefiningMember { get; set; }

        public Disambiguation Disambiguation { get; set; }

        public abstract TokenRef MainTokenRef { get; }

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
    }
}
