using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using IronText.Automata.Regular;
using IronText.Lib.RegularAst;
using IronText.Lib.ScannerExpressions;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    public class ScannerDescriptor
    {
        public static ScannerDescriptor FromScanRules(IEnumerable<Matcher> matchers, ILogging logging)
        {
            CheckAllRulesHaveIndex(matchers);

            var result = new ScannerDescriptor(logging);
            foreach (var matcher in matchers)
            {
                result.AddRule(matcher);
            }

            return result;
        }

        private readonly ILogging logging;
        private readonly List<Matcher> matchers = new List<Matcher>();

        private ScannerDescriptor(ILogging logging) 
        { 
            this.logging = logging;
        }

        public ReadOnlyCollection<Matcher> Matchers { get { return matchers.AsReadOnly(); } }

        public void AddRule(Matcher matcher) { matchers.Add(matcher); }

        public AstNode MakeAst() { return MakeAst(null); }

        public AstNode MakeAst(Dictionary<string,int> literalToAction)
        {
            var descriptor = this;

            var alternatives = new AstNode[descriptor.Matchers.Count];
            var pattern = new StringBuilder(128);

            pattern.Append("(");
            bool first = true;
            foreach (var matcher in descriptor.Matchers)
            {
                if (literalToAction != null && matcher.Pattern.IsLiteral)
                {
                    literalToAction.Add(matcher.Pattern.Literal, matcher.Index);
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
                        .Append(matcher.Pattern.Pattern)
                    .Append(" )")
                        .Append(' ')
                    .Append("action(").Append(matcher.Index).Append(")");
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

            foreach (var matcher in Matchers)
            {
                var binding = matcher.Joint.The<CilMatcher>();

                if (matcher.Pattern.Literal != null)
                {
                    if (matcher.Pattern.Literal == "")
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Message = string.Format(
                                            "Literal cannot be empty string.",
                                            matcher),
                                Origin = ReflectionUtils.ToString(binding.DefiningMethod)
                            });
                    }
                }
                else
                {
                    var ast = GetAst(matcher.Pattern.Pattern);
                    if (IsNullable(ast))
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Message = string.Format(
                                            "Scan pattern cannot match empty string.",
                                            matcher),
                                Origin = ReflectionUtils.ToString(binding.DefiningMethod)
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
                interp.CustomLogging = this.logging;
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

        private static void CheckAllRulesHaveIndex(IEnumerable<Matcher> matchers)
        {
            foreach (var matcher in matchers)
            {
                if (matcher.Index < 0)
                {
                    throw new ArgumentException("Matcher " + matcher + " has no index.", "rules");
                }
            }
        }

    }
}
