﻿using IronText.Build;
using IronText.Logging;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Runtime;
using IronText.Runtime.Semantics;
using IronText.Tests.Algorithm;
using System;
using System.Collections.Generic;
using System.IO;

namespace IronText.Tests.TestUtils
{
    class ParserSut
    {
        private readonly Grammar grammar;
        private readonly string outputDirectory;
        private LanguageData data;

        public ParserSut(Grammar grammar, string outputDirectory = null)
        {
            this.outputDirectory = outputDirectory ?? Environment.CurrentDirectory;
            this.grammar = grammar;

            BuildTables();

            this.ProductionHooks = new Dictionary<string, ReductionAction>();
        }

        private void BuildTables()
        {
            var source = new DirectGrammarSource(outputDirectory, grammar, "test language");
            var provider = new LanguageDataProvider(source, false);
            if (!ResourceContext.Instance.LoadOrBuild(provider))
            {
                throw new InvalidOperationException("Unable to build language data.");
            }

            this.data = provider.Resource;
        }

        public void Parse(string text, Dictionary<int, object> inhIndexToValue = null)
        {
            Parse(new StringReader(text), Loc.MemoryString, inhIndexToValue);
        }

        public void Parse(StringReader input, string document, Dictionary<int, object> inhIndexToValue = null)
        {
            var logging = ExceptionLogging.Instance;

            var producer = new ActionProducer(
                                    data.RuntimeGrammar,
                                    null,
                                    ProductionAction,
                                    TermFactory,
                                    ShiftAction,
                                    null,
                                    inhIndexToValue);

            IReceiver<Msg> parser;
            if (data.IsDeterministic)
            {
                parser = new DeterministicParser<ActionNode>(
                            producer,
                            data.RuntimeGrammar,
                            Transition,
                            logging);
            }
            else
            {
                parser = new RnGlrParser<ActionNode>(
                            data.RuntimeGrammar,
                            data.TokenComplexity,
                            Transition,
                            data.StateToToken,
                            data.ParserConflictActionTable,
                            producer,
                            logging);
            }

            var scanSimulation = new TdfaSimulation(data.ScannerTdfa);
            char[] buffer = new char[1024];
            int len = input.ReadBlock(buffer, 0, buffer.Length - 1);
            buffer[len] = Scanner.Sentinel;

            foreach (var msg in ScanAll(scanSimulation, buffer))
            {
                parser = parser.Next(msg);
                if (parser == null)
                {
                    break;
                }
            }

            if (parser != null)
            {
                parser.Done();
            }
        }

        public Dictionary<string, ReductionAction> ProductionHooks { get; private set; }

        private void ShiftAction(IStackLookback<ActionNode> lookback)
        {
        }

        private object ProductionAction(ProductionActionArgs pargs)
        {
            object result = null;

            foreach (var pair in ProductionHooks)
            {
                var prodName = pair.Key;
                var action = pair.Value;

                var hookProd = grammar.Productions.Find(prodName);
                var prod = grammar.Productions[pargs.ProductionIndex];
                if (hookProd != prod)
                {
                    continue;
                }

                action(pargs);

                foreach (var formula in prod.Semantics)
                {
                    if (formula.IsCalledOnReduce && formula.IsCopy)
                    {
                        //semanticAction.Invoke(pargs);
                    }
                }
            }

            return result;
        }

        private static object TermFactory(object context, int action, string text)
        {
            return null;
        }

        private int Transition(int state, int token)
        {
            return data.ParserActionTable.Get(state, token);
        }

        private static bool TryNextWithTunnels(ITdfaSimulation automaton, int state, int item, out int nextState)
        {
            int currentState = state;

            do
            {
                if (automaton.TryNext(currentState, item, out nextState))
                {
                    return true;
                }
            }
            while (automaton.Tunnel(currentState, out currentState));

            return false;
        }

        private IEnumerable<Msg> ScanAll(ITdfaSimulation automaton, char[] input)
        {
            int cursor = 0;
            int state = automaton.Start;
            int marker = 0;
            int start = 0;
            int acceptingState = automaton.IsAccepting(state) ? state : -1;

            while (true)
            {
                var item = input[cursor];
                int nextState;

                if (!TryNextWithTunnels(automaton, state, item, out nextState))
                {
                    if (acceptingState < 0)
                    {
                        var msg = string.Format("Scan failed at {0} position.", start);
                        throw new InvalidOperationException(msg);
                    }

                    int? action = automaton.GetAction(acceptingState);
                    if (action.HasValue)
                    {
                        int token = data.MatchActionToToken[action.Value];
                        if (token >= 0)
                        {
                            yield return new Msg(
                                    token,
                                    new string(input, start, (marker - start)),
                                    null,
                                    new Loc(1, start + 1, 1, marker + 1));
                        }
                    }

                    if (item == Scanner.Sentinel)
                    {
                        break;
                    }

                    state = 0;
                    start = cursor = marker;
                    marker = -1;
                    acceptingState = -1;
                }
                else
                {
                    ++cursor;
                    state = nextState;
                    if (automaton.IsAccepting(state))
                    {
                        marker = cursor;
                        acceptingState = state;
                    }
                }
            }
        }

        class DirectGrammarSource : ILanguageSource, IReportDestinationHint
        {
            internal readonly Grammar grammar;
            private string outputDirectory;

            public DirectGrammarSource(string outputDirectory, Grammar grammar, string languageName)
            {
                this.outputDirectory = outputDirectory;
                this.grammar = grammar;
                this.LanguageName = languageName;
                this.FullLanguageName = languageName;
                this.GrammarOrigin = "<uknown>";
            }

            public string LanguageName { get; private set; }

            public string FullLanguageName { get; private set; }

            public string GrammarOrigin { get; private set; }

            public string GrammarReaderTypeName { get { return typeof(DirectGrammarReader).AssemblyQualifiedName; } }

            string IReportDestinationHint.OutputDirectory
            {
                get { return this.outputDirectory; }
            }
        }

        public class DirectGrammarReader : IGrammarReader
        {
            public Grammar Read(ILanguageSource source, ILogging logging)
            {
                return ((DirectGrammarSource)source).grammar;
            }
        }
    }

}
