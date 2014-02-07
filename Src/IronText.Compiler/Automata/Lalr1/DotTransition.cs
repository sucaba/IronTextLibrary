using IronText.Algorithm;

namespace IronText.Automata.Lalr1
{
    public class DotTransition
    {
        public readonly MutableIntSet Tokens;
        public readonly DotState To;

        public DotTransition(MutableIntSet tokens, DotState to)
        {
            this.Tokens = tokens;
            this.To = to;
        }
    }
}
