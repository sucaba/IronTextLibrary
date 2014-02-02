using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Automata.Lalr1;
using IronText.Automata.Regular;
using IronText.Build;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Misc;
using System.Text;
using IronText.Algorithm;
using IronText.Reflection;
using IronText.Compiler;
using IronText.Compiler.Analysis;
using IronText.Analysis;
using IronText.Logging;
using IronText.Reporting;
using IronText.Runtime;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    internal class LanguageDataProvider : ResourceGetter<LanguageData>
    {
        private readonly LanguageName languageName;
        private readonly bool         bootstrap;
        private ILogging              logging;

        public LanguageDataProvider(LanguageName name, bool bootstrap)
        {
            this.languageName = name;
            this.bootstrap = bootstrap;
            Getter = Build;
        }

        private bool Build(ILogging logging, out LanguageData result)
        {
            this.logging = logging;

            result = new LanguageData();

            ICilGrammarBuilder grammarBuilder = new CilGrammarBuilder();
            Grammar grammar = grammarBuilder.Build(languageName, logging);

//            var inliner = new ProductionInliner(grammar);
//            grammar = inliner.Inline();

            var reportBuilders = new List<ReportBuilder>(grammarBuilder.ReportBuilders);

            if (!bootstrap)
            {
                if (!CompileScannerTdfas(grammar))
                {
                    result = null;
                    return false;
                }
            }

            // Build parsing tables
            ILrDfa parserDfa = null;

            var grammarAnalysis = new GrammarAnalysis(grammar);
            logging.WithTimeLogging(
                languageName.Name,
                languageName.DefinitionType,
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

            var flags = Attributes.First<LanguageAttribute>(languageName.DefinitionType).Flags;
            var lrTable = new ConfigurableLrTable(parserDfa, flags);
            if (!lrTable.ComplyWithConfiguration)
            {
                reportBuilders.Add(
                    reportData =>
                    {
                        var messageBuilder = new ConflictMessageBuilder(reportData);
                        messageBuilder.Write(logging);
                    });
            }

            var localParseContexts = CollectLocalContexts(grammar, parserDfa);

            // Prepare language data for the language assembly generation
            result.IsDeterministic     = !lrTable.RequiresGlr;
            result.DefinitionType      = languageName.DefinitionType;
            result.Grammar             = grammar;
            result.GrammarAnalysis     = grammarAnalysis;
            result.ParserStates        = parserDfa.States;
            result.StateToSymbolTable  = parserDfa.GetStateToSymbolTable();
            result.ParserActionTable   = lrTable.GetParserActionTable();
            result.ParserConflictActionTable = lrTable.GetConflictActionTable();

            result.LocalParseContexts  = localParseContexts.ToArray();

            if (!bootstrap)
            {
                IReportData reportData = new ReportData(languageName, result, lrTable.Conflicts);
                foreach (var reportBuilder in reportBuilders)
                {
                    reportBuilder(reportData);
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
                            Member = languageName.DefinitionType,
                            Message = string.Format(
                                        "Unable to create scanner for '{0}' language.",
                                        languageName.DefinitionType)
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
            var descr = ScannerDescriptor.FromScanRules(
                                        condition.Name,
                                        condition.Matchers,
                                        logging);

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

        private static List<ProductionContextLink> CollectLocalContexts(Grammar grammar, ILrDfa lrDfa)
        {
            var result = new List<ProductionContextLink>();

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
                        var action = (SimpleProductionAction)consumingProd.Action;

                        if (provider.ProvidedContexts.Contains(action.Context))
                        {
                            result.Add(
                                new ProductionContextLink
                                {
                                    ParentState = parentState,
                                    ContextTokenLookback = item.Position,
                                    Joint = 
                                    {
                                        provider.Joint.The<CilContextProvider>(),
                                        action.Context.Joint.Get<CilContextConsumer>(),
                                    }
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
                && object.Equals(casted.languageName, languageName);
        }

        public override int GetHashCode()
        {
            return languageName.GetHashCode();
        }

        public override string ToString()
        {
            return "LanguageData for " + languageName.DefinitionType.FullName;
        }
    }
}
