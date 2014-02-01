using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using IronText.Automata.Regular;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.RegularAst;
using IronText.Lib.ScannerExpressions;
using IronText.Logging;
using IronText.Runtime;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    public class ScannerDescriptor
    {
        private readonly ILogging logging;

        public static ScannerDescriptor FromScanRules(
            string name,
            IEnumerable<Matcher> scanProductions,
            ILogging logging)
        {
            CheckAllRulesHaveIndex(scanProductions);

            var result = new ScannerDescriptor(name, logging);
            foreach (var scanProduction in scanProductions)
            {
                result.AddRule(scanProduction);
            }

            return result;
        }

        private readonly List<Matcher> productions = new List<Matcher>();

        public ScannerDescriptor(string name, ILogging logging) 
        { 
            this.Name = name;
            this.logging = logging;
        }

        public string Name { get; set; }

        public ReadOnlyCollection<Matcher> Productions { get { return productions.AsReadOnly(); } }

        public void AddRule(Matcher production) { productions.Add(production); }

        public AstNode MakeAst()
        {
            return MakeAst(null);
        }

        public AstNode MakeAst(Dictionary<string,int> literalToAction)
        {
            var descriptor = this;

            var alternatives = new AstNode[descriptor.Productions.Count];
            var pattern = new StringBuilder(128);

            pattern.Append("(");
            bool first = true;
            foreach (var scanRule in descriptor.Productions)
            {
                if (literalToAction != null && scanRule.Pattern.IsLiteral)
                {
                    literalToAction.Add(scanRule.Pattern.Literal, scanRule.Index);
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
                        .Append(scanRule.Pattern.Pattern)
                    .Append(" )")
                        .Append(' ')
                    .Append("action(").Append(scanRule.Index).Append(")");
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

            foreach (var scanProduction in Productions)
            {
                var binding = scanProduction.Joint.The<CilMatcher>();

                if (scanProduction.Pattern.Literal != null)
                {
                    if (scanProduction.Pattern.Literal == "")
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Message = string.Format(
                                            "Literal cannot be empty string.",
                                            scanProduction),
                                Member = binding.DefiningMethod
                            });
                    }
                }
                else
                {
                    var ast = GetAst(scanProduction.Pattern.Pattern);
                    if (IsNullable(ast))
                    {
                        logging.Write(
                            new LogEntry
                            {
                                Severity = Severity.Error,
                                Message = string.Format(
                                            "Scan pattern cannot match empty string.",
                                            scanProduction),
                                Member = binding.DefiningMethod
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

        private static void CheckAllRulesHaveIndex(IEnumerable<Matcher> scanProductions)
        {
            foreach (var scanProduction in scanProductions)
            {
                if (scanProduction.Index < 0)
                {
                    throw new ArgumentException("Rule " + scanProduction + " has no index.", "rules");
                }
            }
        }

    }
}
