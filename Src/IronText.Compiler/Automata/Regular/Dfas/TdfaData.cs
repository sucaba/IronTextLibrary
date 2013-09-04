using System.Collections.Generic;
using System.Diagnostics;
using IronText.Algorithm;
using IronText.Diagnostics;
using IronText.Extensibility;
using System.Collections.ObjectModel;

namespace IronText.Automata.Regular
{
    public sealed class TdfaData 
        : ITdfaData
        , IScannerAutomata
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
            var outgoing = Dstates[from].Outgoing;
            foreach (var transition in outgoing)
            {
                if (transition.To == to)
                {
                    transition.Symbols.Add(symbol);
                    return;
                }
            }

            outgoing.Add(new TdfaTransition(from, Alphabet.SymbolSetType.Of(symbol).EditCopy(), to));
        }

        public void DeleteTransition(int from, int symbol)
        {
            var outgoing = Dstates[from].Outgoing;
            for (int i = 0; i != outgoing.Count; ++i)
            {
                var transition = outgoing[i];
                if (transition.Symbols.Contains(symbol))
                {
                    transition.Symbols.Remove(symbol);
                    if (transition.Symbols.IsEmpty)
                    {
                        outgoing.RemoveAt(i);
                    }

                    break;
                }
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
                    view.AddNode(S.Index, S.Index.ToString(), style: Style.Bold, color: color);
                }
                else
                {
                    view.AddNode(S.Index, S.Index.ToString(), color: color);
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
