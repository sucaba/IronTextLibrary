using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronText.Algorithm;
using System.Collections.ObjectModel;

namespace IronText.Extensibility
{
    public class ScanMode
    {
        internal readonly List<IScanRule> scanRules = new List<IScanRule>();

        internal ScanMode(Type scanModeType)
        {
            this.ScanModeType = scanModeType;
            this.ScanRules = new ReadOnlyCollection<IScanRule>(this.scanRules);
        }

        public Type ScanModeType { get; private set; }

        // Ordered scan rules
        public ReadOnlyCollection<IScanRule> ScanRules { get; private set; }

        internal IScanRule AddLiteralRule(string literal)
        {
            var result = CreateImplicitLiteralScanRule(literal);
            scanRules.Add(result);

            return result;
        }

        internal void AddRule(IScanRule rule)
        {
            scanRules.Add(rule);
        }

        private static ScanRule CreateImplicitLiteralScanRule(string literal)
        {
            // Generate implicit scan rule for the keyword
            var result  = new SingleTokenScanRule
            {
                LiteralText           = literal,
                Pattern               = ScannerUtils.Escape(literal),
                BootstrapRegexPattern = Regex.Escape(literal),
                ActionBuilder = code =>
                {
                    code
                        .Emit(il => il.Ldnull())
                        .ReturnFromAction();
                }
            };

            return result;
        }
    }
}
