using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronText.Algorithm;
using System.Collections.ObjectModel;
using IronText.Framework;

namespace IronText.Extensibility
{
    public class ScanMode
    {
        internal readonly List<IScanRule> scanRules = new List<IScanRule>();
        private int implicitRulesCount = 0;

        internal ScanMode(Type scanModeType)
        {
            this.ScanModeType = scanModeType;
            this.ScanRules = new ReadOnlyCollection<IScanRule>(this.scanRules);
        }

        public Type ScanModeType { get; private set; }

        public int ActionIndexShift { get; internal set; }

        // Ordered scan rules
        public ReadOnlyCollection<IScanRule> ScanRules { get; private set; }

        internal IScanRule AddImplicitLiteralRule(int ruleIndex, string literal)
        {
            var result = CreateImplicitLiteralRule(literal);
            result.Index = ruleIndex;
            scanRules.Insert(implicitRulesCount++, result);

            return result;
        }

        internal void AddRule(IScanRule rule)
        {
            scanRules.Add(rule);
        }

        private static IScanRule CreateImplicitLiteralRule(string literal)
        {
            // Generate implicit scan rule for the keyword
            var result  = new SingleTokenScanRule
            {
                LiteralText           = literal,
                Disambiguation        = Disambiguation.Exclusive,
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
