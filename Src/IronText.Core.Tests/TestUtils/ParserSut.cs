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
            var logging   = new TextLogging(Console.Out);
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
            foreach (var msg in ScanAll(scanSimulation, input.ReadToEnd().ToCharArray()))
            {
                parser = parser.Next(msg);
                if (parser == null)
                {
                    break;
                }
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

        public static int Scan1Delegate(ScanCursor cursor)
        {
            throw new NotImplementedException();
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
            int state = automaton.Start;
            int acceptPos = -1;
            int startPos  = 0;
            int? acceptingState = null;
            for (int pos = 0; pos != input.Length;)
            {
                var item = input[pos];
                int nextState;

                if (!TryNextWithTunnels(automaton, state, item, out nextState))
                {
                    if (!acceptingState.HasValue)
                    {
                        var msg = string.Format("Scan failed at {0} position.", acceptPos);
                        throw new InvalidOperationException(msg);
                    }

                    int? action = automaton.GetAction(acceptingState.Value);
                    if (action.HasValue)
                    {
                        int token = data.MatchActionToToken[action.Value];
                        if (token >= 0)
                        {
                            yield return new Msg(
                                    token,
                                    new string(input, startPos, (acceptPos - startPos)),
                                    null,
                                    new Loc(startPos, acceptPos));
                        }
                    }

                    state = 0;
                    startPos = pos = acceptPos;
                    acceptPos = -1;
                    acceptingState = null;
                }
                else
                {
                    ++pos;
                    state = nextState;
                    if (automaton.IsAccepting(state))
                    {
                        acceptPos      = pos;
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
