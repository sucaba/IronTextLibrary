using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using IronText.Automata.Regular;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.RegularAst;
using IronText.Lib.ScannerExpressions;
using IronText.Logging;

namespace IronText.MetadataCompiler
{
    public class ScannerDescriptor
    {
        private readonly ILogging logging;

        public static ScannerDescriptor FromScanRules(string name, IEnumerable<ScanRule> rules, ILogging logging)
        {
            var result = new ScannerDescriptor(name, logging);
            foreach (var rule in rules)
            {
                result.AddRule(rule);
            }

            return result;
        }

        private readonly List<ScanRule> rules = new List<ScanRule>();

        public ScannerDescriptor(string name, ILogging logging) 
        { 
            this.Name = name;
            this.logging = logging;
        }

        public string Name { get; set; }

        public ReadOnlyCollection<ScanRule> Rules { get { return rules.AsReadOnly(); } }

        public void AddRule(ScanRule rule) { rules.Add(rule); }

        public AstNode MakeAst()
        {
            return MakeAst(null);
        }

        public AstNode MakeAst(Dictionary<string,int> literalToAction)
        {
            var descriptor = this;

            var alternatives = new AstNode[descriptor.Rules.Count];
            int i = 0;
            var pattern = new StringBuilder(1024);

            pattern.Append("(");
            bool first = true;
            foreach (var scanRule in descriptor.Rules)
            {
                if (literalToAction != null && scanRule.LiteralText != null)
                {
                    literalToAction.Add(scanRule.LiteralText, i++);
                    continue;
                }

                if (first)
                {
                    first = false;
                }
                else
                {
                    pattern.Append(" | ");
                }

                pattern
                    .Append("( ")
                        .Append(scanRule.Pattern)
                    .Append(" )")
                        .Append(' ')
                    .Append("action(").Append(i).Append(")");
                ++i;
            }

            if (pattern.Length == 1)
            {
                pattern.Append("[]");
            }

            pattern.Append(")");

            AstNode root = GetAst(pattern.ToString());

            if (CheckNonNullablePatterns(root))
            {
                return root;
            }

            return null;
        }

        private bool CheckNonNullablePatterns(AstNode root)
        {
            if (!IsNullable(root))
            {
                return true;
            }

            foreach (var scanRule in Rules)
            {
                if (scanRule.LiteralText != null)
                {
                    if (scanRule.LiteralText == "")
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Message = string.Format(
                                            "Literal cannot be empty string.",
                                            scanRule),
                                Member = scanRule.DefiningMember
                            });
                    }
                }
                else
                {
                    var ast = GetAst(scanRule.Pattern);
                    if (IsNullable(ast))
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Message = string.Format(
                                            "Scan pattern cannot match empty string.",
                                            scanRule),
                                Member = scanRule.DefiningMember
                            });
                    }
                }
            }

            return false;
        }

        private static AstNode GetAst(string pattern)
        {
            var context = new ScannerSyntax();
            AstNode root = Language.Parse(context, pattern).Result.Node;
            return root;
        }

        private static bool IsNullable(AstNode root)
        {
            bool result = root.Accept(NullableGetter.Instance);
            return result;
        }

    }
}
