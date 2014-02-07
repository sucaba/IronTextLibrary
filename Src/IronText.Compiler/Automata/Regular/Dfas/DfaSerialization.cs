using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Algorithm;

namespace IronText.Automata.Regular
{
    public class DfaSerialization : IDfaSerialization
    {
        private readonly ITdfaData data;

        public DfaSerialization(ITdfaData data)
        {
            this.data     = data;
        }

        public int StateCount { get { return data.StateCount; } }

        public int Start { get { return 0; } }

        public TdfaState GetState(int state) { return data.GetState(state); }

        public IEnumerable<TdfaState> EnumerateStates()
        {
            return data.EnumerateStates();
        }

        public IEnumerable<IntArrow<int>> EnumerateRealTransitions(TdfaState S)
        {
            int state = S.Index;

            foreach (var t in S.Outgoing)
            {
                var intervalSet = (MutableIntervalIntSet)data.Alphabet.Decode(t.Symbols);
                foreach (var interval in intervalSet.EnumerateIntervals())
                {
                    yield return new IntArrow<int>(interval, t.To);
                }
            }
        }

        public void Print(StringBuilder output)
        {
            output
                .AppendFormat("StateCount = {0}", StateCount).AppendLine()
                .AppendFormat("Start = {0}", Start).AppendLine()
                .AppendLine("InputTranslations = [");

            foreach (var symbol in data.Alphabet.Symbols)
            {
                var intervalSet = (IntervalIntSet)data.Alphabet.Decode(symbol);
                foreach (var interval in intervalSet.EnumerateIntervals())
                {
                    output.AppendFormat("   {0} -> {1},", interval, symbol).AppendLine();
                }
            }

            output
                .AppendLine("]")
                .AppendLine("Transitions = [");

            foreach (var state in data.EnumerateStates())
            foreach (var transition in state.Outgoing)
            {
                output
                    .AppendFormat("   {0} ={1}=> {2},", transition.From, transition.Symbols, transition.To)
                    .AppendLine();
            }

            var acceptingStates = EnumerateStates().Where(s => s.IsAccepting).Select(s => s.Index); ;
            output
                .AppendLine("]")
                .Append("AcceptingStates = [").Append(string.Join(", ", acceptingStates)).AppendLine("]")
                .AppendLine("StateToAction = [");
                ;

            foreach (var state in EnumerateStates())
            {
                foreach (var action in state.Actions)
                {
                    output.AppendFormat("   {0} -> {1},", state.Index, action).AppendLine();
                }
            }

            output.AppendLine("]");
        }
    }
}
