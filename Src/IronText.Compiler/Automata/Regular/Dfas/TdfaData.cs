using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using IronText.Algorithm;
using IronText.Diagnostics;
using IronText.Reporting;

namespace IronText.Automata.Regular
{
    public sealed class TdfaData : ITdfaData , IScannerAutomata
    {
        public static readonly SparseIntSetType StateSetType = SparseIntSetType.Instance;
        public static readonly SparseIntSetType PositionSetType = SparseIntSetType.Instance;

        private readonly List<TdfaState> Dstates = new List<TdfaState>();
        private readonly IRegularAlphabet alphabet;

        public TdfaData(IRegularAlphabet alphabet)
        {
            this.alphabet = alphabet;
        }

        public IRegularAlphabet Alphabet { get { return this.alphabet; } }

        public int Start { get { return 0; } }

        public int StateCount { get { return Dstates.Count; } }

        public TdfaState GetState(int state) 
        {
            if (state < 0)
            {
                return null;
            }

            return Dstates[state]; 
        }

        public IEnumerable<TdfaTransition> EnumerateIncoming(int destState)
        {
            foreach (var state in Dstates)
            {
                foreach (var t in state.Outgoing)
                {
                    if (t.To == destState)
                    {
                        yield return t;
                    }
                }
            }
        }

        public void AddTransition(int from, int symbol, int to)
        {
            AddTransition(from, Alphabet.SymbolSetType.Of(symbol), to);
        }

        public void AddTransition(int from, IntSet symbols, int to)
        {
            var outgoing = Dstates[from].Outgoing;
            foreach (var transition in outgoing)
            {
                if (transition.To == to)
                {
                    transition.Symbols.AddAll(symbols);
                    return;
                }
            }

            outgoing.Add(new TdfaTransition(from, symbols.EditCopy(), to));
        }

        public void DeleteTransition(int from, int symbol)
        {
            DeleteTransition(from, Alphabet.SymbolSetType.Of(symbol));
        }

        public void DeleteTransition(int from, IntSet cset)
        {
            var outgoing = Dstates[from].Outgoing;
            for (int i = 0; i != outgoing.Count;)
            {
                var transition = outgoing[i];
                if (transition.Symbols.Overlaps(cset))
                {
                    transition.Symbols.RemoveAll(cset);
                    if (transition.Symbols.IsEmpty)
                    {
                        outgoing.RemoveAt(i);
                        continue;
                    }
                }

                ++i;
            }
        }

        public int AddState(TdfaState dfaState)
        {
            Debug.Assert(dfaState != null);
            Debug.Assert(dfaState.Tunnel == 0);

            int result = Dstates.Count;
            dfaState.Index = result;
            dfaState.Tunnel = -1;
            Dstates.Add(dfaState);
            return result;
        }

        public IEnumerable<TdfaState> EnumerateStates()
        {
            for (int i = 0; i != Dstates.Count; ++i)
            {
                yield return Dstates[i];
            }
        }

        public void DescribeGraph(IGraphView view)
        {
            var data = this;

            view.BeginDigraph("tdfa");

            view.SetGraphProperties(rankDir: RankDir.LeftToRight);
            foreach (var S in data.EnumerateStates())
            {
                GraphColor color = S.IsNewline ? GraphColor.Green : GraphColor.Default;
                if (S.IsAccepting)
                {
                    view.AddNode(S.Index, GetStateName(S), style: Style.Bold, color: color);
                }
                else
                {
                    view.AddNode(S.Index, GetStateName(S), color: color);
                }
            }

            foreach (var S in data.EnumerateStates())
            {
                foreach (var t in S.Outgoing)
                {
                    var charSet = data.Alphabet.Decode(t.Symbols);
                    view.AddEdge(t.From, t.To, charSet.ToCharSetString());
                }

                if (S.Tunnel >= 0)
                {
                    view.AddEdge(S.Index, S.Tunnel, style: Style.Dotted);
                }
            }

            view.EndDigraph();
        }

        private static string GetStateName(TdfaState S)
        {
            var output = new StringBuilder();
            output.Append(S.Index);
            if (S.Actions.Count != 0)
            {
                output.Append(" [");
                output.Append(string.Join(",", S.Actions));
                output.Append("]");
            }

            return output.ToString();
        }

        ReadOnlyCollection<IScannerState> IScannerAutomata.States
        {
            get 
            {
                return new ReadOnlyCollection<IScannerState>(
                            (IList<IScannerState>)(IList<TdfaState>)this.Dstates);
            }
        }
    }
}
