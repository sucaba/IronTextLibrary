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
        private readonly List<IScanRule> scanRules = new List<IScanRule>();

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

        internal void SortScanRules()
        {
            // Sort scan rules 
            var sortedScanRules = scanRules.Where(rule => rule.IsSortable).ToArray();
            var nonSortedScanRules = scanRules.Where(rule => !rule.IsSortable).ToArray();

            // Sort fixed-text tokens to prioritize longest matches:
            Sorting.SpecializationSort(
                sortedScanRules,
                ScanRule.IsMoreSpecialized);

            // Sort rules in the same order as they appear in definition:
            Array.Sort(nonSortedScanRules, ScanRule.ComparePriority);
            scanRules.Clear();
            scanRules.AddRange(sortedScanRules.Concat(nonSortedScanRules));
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
