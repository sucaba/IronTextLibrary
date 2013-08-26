using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronText.Algorithm;

namespace IronText.Extensibility
{
    public class ScanMode
    {
        public Type ScanModeType;

        // Ordered scan rules
        public List<ScanRule> ScanRules;

        internal void SortScanRules()
        {
            // Sort scan rules 
            var sortedScanRules = ScanRules.Where(rule => rule.IsSortable).ToArray();
            var nonSortedScanRules = ScanRules.Where(rule => !rule.IsSortable).ToArray();

            // Sort fixed-text tokens to prioritize longest matches:
            Sorting.SpecializationSort(
                sortedScanRules,
                ScanRule.IsMoreSpecialized);

            // Sort rules in the same order as they appear in definition:
            Array.Sort(nonSortedScanRules, ScanRule.ComparePriority);
            ScanRules.Clear();
            ScanRules.AddRange(sortedScanRules.Concat(nonSortedScanRules));
        }

        public ScanRule AddLiteralRule(string literal)
        {
            var result = CreateImplicitLiteralScanRule(literal);
            ScanRules.Add(result);

            return result;
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
