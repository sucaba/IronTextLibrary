using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using IronText.Automata.Regular;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.MetadataCompiler;
using IronText.Runtime;
using IronText.Logging;

namespace IronText.Tests.Algorithm
{
    /// <summary>
    /// Lexer class to test and debug DFA semantics.
    /// </summary>
    public class DfaSimulationLexer : ISequence<Msg>
    {
        private IDfaSimulation dfa;
        private readonly string text;
        private ScannerDescriptor descriptor;

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

                var matcher = descriptor.Matchers[action];
                if (matcher.Outcome != null)
                {
                    var detOutcome = (Symbol)matcher.Outcome; // Only deterministic symbols supported

                    // Emit next token
                    visitor = visitor.Next(
                        new Msg(detOutcome.Index, text.Substring(start, tokenLength), null, new Loc(Loc.MemoryString, start, pos)));
                }

                start = pos;
            }

            return visitor.Done();
        }
    }
}
