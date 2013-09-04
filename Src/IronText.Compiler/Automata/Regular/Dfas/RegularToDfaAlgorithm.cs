using System.Linq;
using System.Text;
using IronText.Extensibility;

namespace IronText.Automata.Regular
{
    public sealed class RegularToDfaAlgorithm
    {
        private readonly IRegularAlphabet alphabet;
        private readonly ITdfaData data;

        public RegularToDfaAlgorithm(RegularTree regTree)
            : this(regTree, new EquivalenceClassesAlphabet(regTree.GetEquivalenceCsets()))
            // this(regTree, new RegularAlphabet(regTree.Positions.Select(node => node.Characters)))
        {
        }

        public RegularToDfaAlgorithm(RegularTree regTree, IRegularAlphabet alphabet)
        {
            this.alphabet = alphabet;

            data = new TdfaData(alphabet); 

            data.AddState(new TdfaState(data) { Positions = regTree.FirstPos });

            foreach (var st in data.EnumerateStates())
            {
                int Sindex = st.Index;
                var S = st.Positions;
                if (S.Contains(regTree.EoiPosition))
                {
                    data.GetState(Sindex).IsAccepting = true;
                }

                int? stateAction = null;
                int actionPos = int.MaxValue;
                foreach (var position in S)
                {
                    var action = regTree.GetPosAction(position);
                    if (action.HasValue)
                    {
                        // Resolve ambiguous actions by selecting the first one (lowest position in regexp)
                        if (!stateAction.HasValue || actionPos > position)
                        {
                            stateAction = action;
                            actionPos = position;
                        }
                    }
                }

                if (stateAction.HasValue)
                {
                    st.Action = stateAction;
                }

                var transitionSymbols = alphabet.SymbolSetType.Union(
                                            st.Positions
                                            .Select(regTree.GetPosSymbols)
                                            .Select(alphabet.Encode));
                foreach (var symbol in transitionSymbols)
                {
                    if (symbol == alphabet.EoiSymbol)
                    {
                        continue;
                    }
                    
                    var U = TdfaData.PositionSetType.Mutable();
                    foreach (var position in S)
                    {
                        var cset = alphabet.Encode(regTree.GetPosSymbols(position));
                        if (cset.Contains(symbol))
                        {
                            U.AddAll(regTree.GetFollowPos(position));
                        }
                    }

                    if (!U.IsEmpty)
                    {
                        int Uindex = data.IndexOfState(U);
                        if (Uindex < 0)
                        {
                            Uindex = data.AddState(new TdfaState(data) { Positions = U });
                        }

                        data.AddTransition(from: Sindex, symbol: symbol, to: Uindex);
                    }
                }
            }
        }

        public ITdfaData Data { get { return data; } }

        public IRegularAlphabet Alphabet { get { return alphabet; } } 

        public override string ToString()
        {
            var output = new StringBuilder();
            foreach (var state in data.EnumerateStates())
            foreach (var transition in state.Outgoing)
            {
                output
                    .AppendFormat(
                        "{0} --{1}--> {2}",
                        data.GetState(transition.From).Positions,
                        alphabet.Decode(transition.Symbols),
                        data.GetState(transition.To).Positions);

                if (data.GetState(transition.To).IsAccepting)
                {
                    output.Append(" [CAN ACCEPT] ");
                }
                
                output
                    .AppendLine();
            }

            return output.ToString();
        }
    }
}
