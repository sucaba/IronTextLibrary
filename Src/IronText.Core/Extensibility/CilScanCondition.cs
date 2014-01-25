using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronText.Algorithm;
using System.Collections.ObjectModel;
using IronText.Framework;

namespace IronText.Extensibility
{
    public class CilScanCondition
    {
        private readonly List<ICilScanRule> scanRules = new List<ICilScanRule>();
        private int implicitRulesCount = 0;

        internal CilScanCondition(Type scanModeType)
        {
            this.ScanModeType = scanModeType;
            this.ScanRules = new ReadOnlyCollection<ICilScanRule>(this.scanRules);
        }

        public Type ScanModeType { get; private set; }

        // Ordered scan rules
        public ReadOnlyCollection<ICilScanRule> ScanRules { get; private set; }

        internal ICilScanRule AddImplicitLiteralRule(string literal)
        {
            var result = CreateImplicitLiteralRule(literal);
            scanRules.Insert(implicitRulesCount++, result);

            return result;
        }

        internal void AddRule(ICilScanRule rule)
        {
            scanRules.Add(rule);
        }

        private static ICilScanRule CreateImplicitLiteralRule(string literal)
        {
            // Generate implicit scan rule for the keyword
            var result  = new CilSingleTokenScanRule
            {
                LiteralText           = literal,
                Disambiguation        = Disambiguation.Exclusive,
                Pattern               = ScannerUtils.Escape(literal),
                BootstrapRegexPattern = Regex.Escape(literal),
                Builder = code =>
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
