using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    internal abstract class ScanRule : IScanRule, IBootstrapScanRule
    {
        public MethodInfo DefiningMember { get; set; }

        public Disambiguation Disambiguation { get; set; }

        public abstract TokenRef MainTokenRef { get; }

        public abstract IEnumerable<TokenRef[]> GetTokenRefGroups();

        public string Pattern { get; set; }

        public string LiteralText { get; set; }

        string IBootstrapScanRule.BootstrapRegexPattern { get { return BootstrapRegexPattern; } }

        internal string BootstrapRegexPattern { get; set; }

        public ScanActionBuilder ActionBuilder { get; set; }

        public Type NextModeType { get; set; }

        // for bootstrap
        internal bool ShouldSkip { get { return this is ISkipScanRule; } }

        private int index = -1;
        int IScanRule.Index { get { return index; } set { index = value; } }

        // for sorting
        internal int Priority { get; set; }

        public override string ToString()
        {
            if (DefiningMember != null)
            {
                return DefiningMember.ToString();
            }
            else if (LiteralText != null)
            {
                return LiteralText;
            }

            return base.ToString();
        }
    }
}
