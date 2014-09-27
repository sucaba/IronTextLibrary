using IronText.Build;
using IronText.Logging;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
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
        }

        private void BuildTables()
        {
            var source   = new DirectGrammarSource(outputDirectory, grammar, "test language");
            var provider = new LanguageDataProvider(source, false);
            if (!ResourceContext.Instance.LoadOrBuild(provider))
            {
                throw new InvalidOperationException("Unable to build language data.");
            }

            this.data = provider.Resource;
        }

        public void Parse(string text)
        {
            Parse(new StringReader(text), Loc.MemoryString);
        }

        public void Parse(StringReader input, string document)
        {
            var logging   = ExceptionLogging.Instance;
            var rtGrammar = new RuntimeGrammar(grammar);
            var producer  = new ActionProducer(rtGrammar, null, ProductionAction, TermFactory, null);
            IReceiver<Msg>   parser;
            if (data.IsDeterministic)
            {
                parser = new DeterministicParser<ActionNode>(
                            producer,
                            rtGrammar,
                            Transition,
                            null,
                            logging);
            }
            else
            {
                parser = new RnGlrParser<ActionNode>(
                            rtGrammar,
                            data.TokenComplexity,
                            Transition,
                            data.StateToToken,
                            data.ParserConflictActionTable,
                            producer,
                            null,
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

        private static object ProductionAction(
            int         ruleId,      // rule being reduced
            ActionNode[] parts,       // array containing path being reduced
            int         firstIndex,  // starting index of the path being reduced
            object      context,     // user provided context
            IStackLookback<ActionNode> lookback    // access to the prior stack states and values
            )
        {
            return null;
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
            int state  = automaton.Start;
            int marker = -1;
            int start  = 0;
            int acceptingState = -1;


            while(true)
            {
                var item = input[cursor];
                int nextState;

                if (!TryNextWithTunnels(automaton, state, item, out nextState))
                {
                    if (acceptingState < 0)
                    {
                        var msg = string.Format("Scan failed at {0} position.", marker);
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
                                    new Loc(start, marker));
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
                        marker      = cursor;
                        acceptingState = state;
                    }
                }
            }
        }

        class DirectGrammarSource : IGrammarSource, IReportDestinationHint
        {
            internal readonly Grammar grammar;
            private string outputDirectory;

            public DirectGrammarSource(string outputDirectory, Grammar grammar, string languageName)
            {
                this.outputDirectory = outputDirectory;
                this.grammar = grammar;
                this.LanguageName = languageName;
                this.FullLanguageName = languageName;
                this.Origin = "<uknown>";
            }

            public string LanguageName { get; private set; }

            public string FullLanguageName { get; private set; }

            public string Origin { get; private set; } 

            public string ReaderTypeName { get { return typeof(DirectGrammarReader).AssemblyQualifiedName; } }

            string IReportDestinationHint.OutputDirectory
            {
                get { return this.outputDirectory; }
            }
        }

        public class DirectGrammarReader : IGrammarReader
        {
            public Grammar Read(IGrammarSource source, ILogging logging)
            {
                return ((DirectGrammarSource)source).grammar;
            }
        }
    }
    
}
