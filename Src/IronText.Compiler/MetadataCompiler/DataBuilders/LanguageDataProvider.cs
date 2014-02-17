using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Build;
using IronText.Compiler.Analysis;
using IronText.Extensibility;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Reporting;

namespace IronText.MetadataCompiler
{
    internal class LanguageDataProvider : ResourceGetter<LanguageData>
    {
        private readonly IGrammarSource source;
        private readonly bool           bootstrap;
        private ILogging                logging;

        public LanguageDataProvider(IGrammarSource source, bool bootstrap)
        {
            this.source    = source;
            this.bootstrap = bootstrap;
            this.Getter    = Build;
        }

        private bool Build(ILogging logging, out LanguageData result)
        {
            this.logging = logging;

            result = new LanguageData();

            var readerType = Type.GetType(source.ReaderTypeName);
            if (readerType == null)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = string.Format(
                                    "Unable to load grammar builder '{0}' for language '{1}'",
                                    source.ReaderTypeName,
                                    source.LanguageName),
                        Origin = source.Origin
                    });
                return false;
            }

            var reader = (IGrammarReader)Activator.CreateInstance(readerType);
            var grammar = reader.Read(source, logging);
            if (grammar == null)
            {
                return false;
            }

            grammar.Joint.Add(source);

//            var inliner = new ProductionInliner(grammar);
//            grammar = inliner.Inline();

            if (!bootstrap && !CompileScannerTdfas(grammar))
            {
                result = null;
                return false;
            }

            // Build parsing tables
            ILrDfa parserDfa = null;

            var grammarAnalysis = new GrammarAnalysis(grammar);
            logging.WithTimeLogging(
                source.LanguageName,
                source.Origin,
                () =>
                {
                    parserDfa = new Lalr1Dfa(grammarAnalysis, LrTableOptimizations.Default);
                },
                "building LALR1 DFA");

            if (parserDfa == null)
            {
                result = null;
                return false;
            }

            var lrTable = new ConfigurableLrTable(parserDfa, grammar.Options);
            if (!lrTable.ComplyWithConfiguration)
            {
                grammar.Reports.Add(new ConflictMessageBuilder(logging));
            }

            var localParseContexts = CollectLocalContexts(grammar, parserDfa);

            // Prepare language data for the language assembly generation
            result.IsDeterministic     = !lrTable.RequiresGlr;
            result.Grammar             = grammar;
            result.TokenComplexity     = grammarAnalysis.GetTokenComplexity();
            result.StateToToken        = parserDfa.GetStateToSymbolTable();
            result.ParserActionTable   = lrTable.GetParserActionTable();
            result.ParserConflictActionTable = lrTable.GetConflictActionTable();

            result.LocalParseContexts  = localParseContexts.ToArray();

            if (!bootstrap)
            {
                IReportData reportData = new ReportData(source, result, lrTable.Conflicts, parserDfa.States);
                foreach (var report in grammar.Reports)
                {
                    report.Build(reportData);
                }
            }

            return true;
        }

        private bool CompileScannerTdfas(Grammar grammar)
        {
            var tokenSet = new BitSetType(grammar.Symbols.Count);

            IScanAmbiguityResolver scanAmbiguityResolver
                                = new ScanAmbiguityResolver(
                                        tokenSet,
                                        grammar.Matchers.Count);

            foreach (var condition in grammar.Conditions)
            {
                ITdfaData tdfa;
                if (!CompileTdfa(logging, condition, out tdfa))
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Origin   = source.Origin,
                            Message  = string.Format(
                                        "Unable to create scanner for '{0}' language.",
                                        source.LanguageName)
                        });

                    return false;
                }

                // For each action store information about produced tokens
                foreach (var scanProduction in condition.Matchers)
                {
                    scanAmbiguityResolver.RegisterAction(scanProduction);
                }

                // For each 'ambiguous scanner state' deduce all tokens
                // which can be produced in this state.
                foreach (var state in tdfa.EnumerateStates())
                {
                    scanAmbiguityResolver.RegisterState(state);
                }
            }

            scanAmbiguityResolver.DefineAmbiguities(grammar);

            return true;
        }

        private static bool CompileTdfa(ILogging logging, Condition condition, out ITdfaData outcome)
        {
            var descr = ScannerDescriptor.FromScanRules(condition.Matchers, logging);

            var literalToAction = new Dictionary<string, int>();
            var ast = descr.MakeAst(literalToAction);
            if (ast == null)
            {
                outcome = null;
                return false;
            }

            var regTree = new RegularTree(ast);
            outcome = new RegularToTdfaAlgorithm(regTree, literalToAction).Data;
            condition.Joint.Add(outcome);

            return true;
        }

        private static List<ProductionContextBinding> CollectLocalContexts(Grammar grammar, ILrDfa lrDfa)
        {
            var result = new List<ProductionContextBinding>();

            var states     = lrDfa.States;
            int stateCount = states.Length;

            for (int parentState = 0; parentState != stateCount; ++parentState)
            {
                foreach (var item in states[parentState].Items)
                {
                    if (item.Position == 0 || item.IsReduce)
                    {
                        // Skip items in which local context cannot be provided.
                        continue;
                    }

                    var providingProd = grammar.Productions[item.ProductionId];
                    var provider      = providingProd.Pattern[0];
                    var childSymbol   = providingProd.Pattern[item.Position];

                    foreach (var consumingProd in childSymbol.Productions)
                    {
                        var action = consumingProd.Actions[0];

                        if (provider.ProvidedContexts.Contains(action.Context))
                        {
                            result.Add(
                                new ProductionContextBinding
                                {
                                    StackState          = parentState,
                                    ProviderStackLookback = item.Position,
                                    Provider             = provider,
                                    Consumer             = action.Context
                                });
                        }
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as LanguageDataProvider;
            return casted != null
                && object.Equals(casted.source, source);
        }

        public override int GetHashCode()
        {
            return source.GetHashCode();
        }

        public override string ToString()
        {
            return "LanguageData for " + source.FullLanguageName;
        }
    }
}
