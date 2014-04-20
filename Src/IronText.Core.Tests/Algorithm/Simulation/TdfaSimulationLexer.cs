using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using IronText.Automata.Regular;
using IronText.Diagnostics;
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
    public class TdfaSimulationLexer : ISequence<Msg>
    {
        private ITdfaSimulation tdfa;
        private readonly string text;
        private ScannerDescriptor descriptor;

        public TdfaSimulationLexer(string text, ScannerDescriptor descriptor)
            : this(new StringReader(text), descriptor)
        { }

        public TdfaSimulationLexer(TextReader textSource, ScannerDescriptor descriptor)
        {
            this.text = textSource.ReadToEnd();
            this.descriptor = descriptor;

            Dictionary<string, int> literalToAction = new Dictionary<string,int>();
            var root = descriptor.MakeAst(literalToAction);
            var regTree = new RegularTree(root);
            var algorithm = new RegularToTdfaAlgorithm(regTree, literalToAction);
            DescribeTdfa(algorithm.Data);
            this.tdfa = new TdfaSimulation(algorithm.Data);
        }

        private void DescribeTdfa(ITdfaData data)
        {
            using (IGraphView view = new GvGraphView("tdfa.gv"))
            {
                data.DescribeGraph(view);
            }
        }

        public IReceiver<Msg> Accept(IReceiver<Msg> visitor)
        {
            int start = 0, pos = 0;

            int length = text.Length;

            while (pos != length)
            {
                int state = tdfa.Start;
                int? acceptingState = null;

                // Find longest token
                for (; pos != length; )
                {
                    int item = text[pos];

                    int next;
                    if (tdfa.TryNext(state, item, out next))
                    {
                        state = next;
                        if (tdfa.IsAccepting(state))
                        {
                            acceptingState = state;
                        }

                        ++pos;
                    }
                    else if (tdfa.Tunnel(state, out next))
                    {
                        state = next;
                    }
                    else
                    {
                        break;
                    }
                }

                // Fail
                if (!acceptingState.HasValue)
                {
                    throw new Exception("Unexpected character at pos " + pos);
                }

                int ruleIndex = tdfa.GetAction(acceptingState.Value) ?? -1;
                int tokenLength = pos - start;

                var prod = descriptor.Matchers[ruleIndex];

                if (prod.Outcome != null)
                {
                    // Emit next token
                    visitor = visitor.Next(
                        new Msg(
                            prod.Outcome.Index,
                            null,
                            text.Substring(start, tokenLength),
                            new Loc(Loc.MemoryString, start, pos)));
                }

                start = pos;
            }

            return visitor.Done();
        }
    }
}
