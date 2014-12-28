using IronText.Build;
using IronText.Logging;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using IronText.Runtime.Producers.Actions;
using IronText.Tests.Algorithm;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            this.ProductionHooks = new Dictionary<string,Delegate>();
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

        public void Parse(string text, Dictionary<string,object> globals = null)
        {
            Parse(new StringReader(text), Loc.MemoryString, globals);
        }

        public void Parse(StringReader input, string document, Dictionary<string,object> globals = null)
        {
            var logging   = ExceptionLogging.Instance;
            var rtGrammar = new RuntimeGrammar(grammar);

            var producer  = new ActionProducer(rtGrammar, null, ProductionAction, TermFactory, null, globals);
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

        public Dictionary<string, Delegate> ProductionHooks { get; private set; }

        private object ProductionAction(ProductionActionArgs pargs)
        {
            object result = null;

            foreach (var pair in ProductionHooks)
            {
                var prodName = pair.Key;
                var action   = pair.Value;

                var hookProd = grammar.Productions.Find(prodName);
                var prod = grammar.Productions[pargs.ProductionIndex];
                if (hookProd != prod)
                {
                    continue;
                }

                object[] args = new object[action.Method.GetParameters().Length];
                bool hasDataContext = HasDataContext(action.Method);
                int outputStartIndex = 0;
                if (hasDataContext)
                {
                    outputStartIndex = 1;
                    args[0] = null; // TODO
                }

                new SemanticArgumentBuilder(pargs, args, outputStartIndex).FillSemanticParameters(prod);
                action.DynamicInvoke(args);
            }

            return result;
        }

        private bool HasDataContext(System.Reflection.MethodInfo methodInfo)
        {
            var param = methodInfo.GetParameters().FirstOrDefault();
            return param != null && param.ParameterType == typeof(IDataContext);
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
            int marker = 0;
            int start  = 0;
            int acceptingState = automaton.IsAccepting(state) ? state : -1;

            while(true)
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
