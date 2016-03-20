﻿using System;
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
using IronText.Reflection.Transformations;
using IronText.Misc;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    internal class LanguageDataProvider : ResourceGetter<LanguageData>
    {
        private readonly ILanguageSource source;
        private readonly bool           bootstrap;
        private ILogging                logging;

        public LanguageDataProvider(ILanguageSource source, bool bootstrap)
        {
            this.source    = source;
            this.bootstrap = bootstrap;
            this.Getter    = Build;
        }

        private bool Build(ILogging logging, out LanguageData result)
        {
            this.logging = logging;

            result = new LanguageData();

            var readerType = Type.GetType(source.GrammarReaderTypeName);
            if (readerType == null)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = string.Format(
                                    "Unable to find grammar reader '{0}' for language '{1}'",
                                    source.GrammarReaderTypeName,
                                    source.LanguageName),
                        Origin = source.GrammarOrigin
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

            var transform = new EliminateRightNulls(grammar);
            transform.Apply();

            grammar.BuildIndexes();

            ITdfaData tdfa = null;
            IEnumerable<AmbTokenInfo> ambiguities = null;
            if (!bootstrap && !CompileScannerTdfas(grammar, out tdfa, out ambiguities))
            {
                result = null;
                return false;
            }

            // Build parsing tables
            ILrDfa parserDfa = null;

            var analysis = new GrammarAnalysis(grammar, ambiguities ?? new AmbTokenInfo[0]);

            int[] matchActionToToken = BuildMatchActionToTokenTable(grammar, ambiguities ?? new AmbTokenInfo[0]);

            logging.WithTimeLogging(
                source.LanguageName,
                source.GrammarOrigin,
                () =>
                {
                    parserDfa = new Lalr1Dfa(analysis, LrTableOptimizations.Default);
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

            var semanticBindings = new List<StackSemanticBinding>();
            CollectStackSemanticBindings(grammar, parserDfa, semanticBindings);

            IStateToFormulasProvider stateToFormulasProvider = new StateToFormulasProvider(grammar, parserDfa);
            var stateToFormuals = stateToFormulasProvider.GetData();

            // Prepare language data for the language assembly generation
            result.IsDeterministic     = !lrTable.RequiresGlr;
            result.Grammar             = grammar;
            result.StateToFormulas     = stateToFormuals;
            result.RuntimeGrammar      = grammar.ToRuntime(stateToFormuals);
            result.Analysis            = analysis;               
            result.TokenComplexity     = analysis.GetTokenComplexity();
            result.StateToToken        = parserDfa.GetStateToSymbolTable();
            result.ParserActionTable   = lrTable.GetParserActionTable();
            result.ParserConflictActionTable = lrTable.GetConflictActionTable();
            result.MatchActionToToken  = matchActionToToken;
            result.ScannerTdfa         = tdfa;

            result.SemanticBindings  = semanticBindings.ToArray();

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

        private int[] BuildMatchActionToTokenTable(Grammar grammar, IEnumerable<AmbTokenInfo> ambiguities)
        {
            var actionToToken = grammar.Matchers.CreateCompatibleArray<int>(IndexingConstants.NoIndex);

            int first = grammar.Matchers.StartIndex;
            int last  = grammar.Matchers.Count;

            for (int i = first; i != last; ++i)
            {
                var outcome = grammar.Matchers[i].Outcome;
                if (outcome == null)
                {
                    actionToToken[i] = IndexingConstants.NoIndex; // Skip tokens like whitespace and comments
                }
                else if (outcome is AmbiguousTerminal)
                {
                    var ambOutcome = outcome as AmbiguousTerminal;
                    actionToToken[i] = (from amb in ambiguities
                                       where Enumerable.SequenceEqual(amb.Alternatives, ambOutcome.Alternatives.Select(alt => alt.Index))
                                       select amb.EnvelopeIndex)
                                       .SingleOrDefault();
                }
                else
                {
                    var detOutcome = (Symbol)outcome;
                    actionToToken[i] = detOutcome.Index;
                }
            }

            return actionToToken;
        }

        private bool CompileScannerTdfas(Grammar grammar, out ITdfaData tdfa, out IEnumerable<AmbTokenInfo> ambiguities)
        {
            if (!CompileTdfa(logging, grammar, out tdfa))
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

                ambiguities = null;
                return false;
            }

            var resolver = new LexicalAmbiguityCollector(grammar, tdfa);
            ambiguities = resolver.CollectAmbiguities();

            return true;
        }

        private static bool CompileTdfa(ILogging logging, Grammar grammar, out ITdfaData outcome)
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

        private static List<StackSemanticBinding> CollectStackSemanticBindings(
            Grammar grammar,
            ILrDfa  lrDfa,
            List<StackSemanticBinding> output)
        {
            var states     = lrDfa.States;
            int stateCount = states.Length;

            for (int parentState = 0; parentState != stateCount; ++parentState)
            {
                foreach (var item in states[parentState].Items)
                {
                    if (item.Position == 0 || item.IsReduce)
                    {
                        // Skip items which cannot provide semantic values.
                        continue;
                    }

                    var providingProd   = grammar.Productions[item.ProductionId];
                    var providingSymbol = providingProd.Input[0];
                    var childSymbol     = providingProd.Input[item.Position];

                    foreach (var consumingProd in childSymbol.Productions)
                    {
                        if (providingSymbol.LocalScope.Lookup(consumingProd.ContextRef))
                        {
                            output.Add(
                                new StackSemanticBinding
                                {
                                    StackState          = parentState,
                                    ProvidingProduction = providingProd,
                                    StackLookback       = item.Position,
                                    ConsumingProduction = consumingProd,
                                    Scope               = providingSymbol.LocalScope,
                                    Reference           = consumingProd.ContextRef
                                });
                        }
                    }
                }
            }

            return output;
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
