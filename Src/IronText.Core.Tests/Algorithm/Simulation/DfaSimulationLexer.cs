using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using IronText.Automata.Regular;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.MetadataCompiler;

namespace IronText.Tests.Algorithm
{
    /// <summary>
    /// Lexer class to test and debug DFA semantics.
    /// </summary>
    public class DfaSimulationLexer : ISequence<Msg>
    {
        private delegate object TokenFactoryDelegate(string text);

        private IDfaSimulation dfa;
        private readonly string text;
        private ScannerDescriptor descriptor;
        // rule index -> token factory mapping:
        private TokenFactoryDelegate[] tokenFactories;

        public DfaSimulationLexer(string text, ScannerDescriptor descriptor)
            : this(new StringReader(text), descriptor)
        { }

        public DfaSimulationLexer(TextReader textSource, ScannerDescriptor descriptor)
        {
            this.text = textSource.ReadToEnd();
            this.descriptor = descriptor;

            var root = descriptor.MakeAst();
            var regTree = new RegularTree(root);
            var algorithm = new RegularToDfaAlgorithm(regTree);
            this.dfa = new DfaSimulation(algorithm.Data);

            int count = descriptor.Productions.Count;
            this.tokenFactories = new TokenFactoryDelegate[count];

            for (int i = 0; i != count; ++i)
            {
                if (descriptor.Productions[i].Outcome != null)
                {
                    tokenFactories[i] = BuildTokenFactory(descriptor.Productions[i]);
                }
            }
        }

        public IReceiver<Msg> Accept(IReceiver<Msg> visitor)
        {
            int start = 0, pos = 0;

            int length = text.Length;

            while (pos != length)
            {
                int state = dfa.Start;
                int? acceptingState = null;

                // Find longest token
                for (; pos != length; ++pos)
                {
                    int item = text[pos];

                    if (!dfa.TryNext(state, item, out state))
                    {
                        break;
                    }

                    if (dfa.IsAccepting(state))
                    {
                        acceptingState = state;
                    }
                }

                // Fail
                if (!acceptingState.HasValue)
                {
                    throw new Exception("Unexpected character at pos " + pos);
                }

                int action = dfa.GetAction(acceptingState.Value) ?? -1;
                int tokenLength = pos - start;

                var production = descriptor.Productions[action];
                if (production.Outcome != null)
                {
                    TokenFactoryDelegate tokenFactory = tokenFactories[action];

                    // Emit next token
                    visitor = visitor.Next(
                        new Msg(
                            production.Outcome.Index,
                            tokenFactory(text.Substring(start, tokenLength)),
                            new Loc(Loc.MemoryString, start, pos)));
                }

                start = pos;
            }

            return visitor.Done();
        }

        private static TokenFactoryDelegate BuildTokenFactory(ScanProduction scanProduction)
        {
            return (string text) => text;
        }
    }
}
