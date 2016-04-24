using IronText.Automata.Regular;
using IronText.Compiler.Analysis;
using IronText.Logging;
using IronText.Reflection;
using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.MetadataCompiler
{
    class ScannerAutomataProvider
    {
        private readonly Grammar grammar;
        private readonly ILogging logging;

        public ScannerAutomataProvider(
            Grammar             grammar,
            ILogging            logging,
            ILanguageSource     source,
            LanguageBuildConfig config)
        {
            this.grammar = grammar;
            this.logging = logging;

            ITdfaData tdfa;

            if (config.IsBootstrap)
            {
                // Bootstrap scanner uses its own scanner without TDFA
                Ambiguities = new AmbTokenInfo[0];
                Success = true;
            }
            else if (!CompileTdfa(out tdfa))
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Origin   = source.GrammarOrigin,
                        Message  = string.Format(
                                    "Unable to create scanner for '{0}' language.",
                                    source.LanguageName)
                    });

                Success = false;
            }
            else
            {
                var resolver = new LexicalAmbiguityCollector(grammar, tdfa);

                Tdfa = tdfa;
                Ambiguities = resolver.CollectAmbiguities();
                Success = true;
            }
        }

        public bool Success { get; }

        public ITdfaData                Tdfa         { get; }

        public IEnumerable<AmbTokenInfo> Ambiguities { get; }

        private bool CompileTdfa(out ITdfaData outcome)
        {
            var descr = ScannerDescriptor.FromScanRules(grammar.Matchers, logging);

            var literalToAction = new Dictionary<string, int>();
            var ast = descr.MakeAst(literalToAction);
            if (ast == null)
            {
                outcome = null;
                return false;
            }

            var regTree = new RegularTree(ast);
            outcome = new RegularToTdfaAlgorithm(regTree, literalToAction).Data;

            return true;
        }
    }
}
