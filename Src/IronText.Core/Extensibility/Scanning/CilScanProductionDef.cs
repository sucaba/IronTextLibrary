using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    internal abstract class CilScanRule : ICilScanRule, ICilBootstrapScanRule
    {
        public MethodInfo DefiningMember { get; set; }

        public Disambiguation Disambiguation { get; set; }

        public abstract CilSymbolRef MainTokenRef { get; }

        public abstract IEnumerable<CilSymbolRef[]> GetTokenRefGroups();

        public string Pattern { get; set; }

        public string LiteralText { get; set; }

        string ICilBootstrapScanRule.BootstrapRegexPattern { get { return BootstrapRegexPattern; } }

        internal string BootstrapRegexPattern { get; set; }

        public CilScanActionBuilder ActionBuilder { get; set; }

        public Type NextModeType { get; set; }

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
