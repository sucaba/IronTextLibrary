using IronText.Build;
using IronText.Logging;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Runtime;
using IronText.Tests.Algorithm;
using System;
using System.IO;

namespace IronText.Tests.TestUtils
{
    class ParserSut
    {
        private Grammar grammar;
        private LanguageData data;

        public ParserSut(Grammar grammar)
        {
            this.grammar = grammar;
            BuildTables();
        }

        private void BuildTables()
        {
            var source   = new DirectGrammarSource(grammar, "test language");
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
            foreach (var msg in scanSimulation.ScanAll(input.ReadToEnd().ToCharArray()))
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

        class DirectGrammarSource : IGrammarSource
        {
            internal readonly Grammar grammar;

            public DirectGrammarSource(Grammar grammar, string languageName)
            {
                this.grammar = grammar;
                this.LanguageName = languageName;
                this.FullLanguageName = languageName;
                this.Origin = "<uknown>";
            }

            public string LanguageName { get; private set; }

            public string FullLanguageName { get; private set; }

            public string Origin { get; private set; } 

            public string ReaderTypeName { get { return typeof(DirectGrammarReader).AssemblyQualifiedName; } }
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
