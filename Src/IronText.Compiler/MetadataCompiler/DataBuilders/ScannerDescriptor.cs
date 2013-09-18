using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using IronText.Automata.Regular;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.RegularAst;
using IronText.Lib.ScannerExpressions;

namespace IronText.MetadataCompiler
{
    public class ScannerDescriptor
    {
        private readonly ILogging logging;

        public static ScannerDescriptor FromScanRules(string name, IEnumerable<IScanRule> rules, ILogging logging)
        {
            var result = new ScannerDescriptor(name, logging);
            foreach (var rule in rules)
            {
                result.AddRule(rule);
            }

            return result;
        }

        private readonly List<IScanRule> rules = new List<IScanRule>();

        public ScannerDescriptor(string name, ILogging logging) 
        { 
            this.Name = name;
            this.logging = logging;
        }

        public string Name { get; set; }

        public ReadOnlyCollection<IScanRule> Rules { get { return rules.AsReadOnly(); } }

        public void AddRule(IScanRule rule) { rules.Add(rule); }

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
                var asSingleTokenRule = scanRule as ISingleTokenScanRule;
                if (asSingleTokenRule != null 
                    && literalToAction != null 
                    && asSingleTokenRule.LiteralText != null)
                {
                    literalToAction.Add(asSingleTokenRule.LiteralText, i++);
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
                var asSingleTokenRule = scanRule as ISingleTokenScanRule;

                if (asSingleTokenRule != null && asSingleTokenRule.LiteralText != null)
                {
                    if (asSingleTokenRule.LiteralText == "")
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Message = string.Format(
                                            "Literal cannot be empty string.",
                                            scanRule),
                                Member = asSingleTokenRule.DefiningMember
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

        private AstNode GetAst(string pattern)
        {
            var context = new ScannerSyntax();
            using (var interp = new Interpreter<ScannerSyntax>(context))
            {
                interp.CustomLog = this.logging;
                if (!interp.Parse(pattern))
                {
                    return AstNode.Stub;
                }

                AstNode root = context.Result.Node;
                return root;
            }
        }

        private static bool IsNullable(AstNode root)
        {
            bool result = root.Accept(NullableGetter.Instance);
            return result;
        }

    }
}
