using IronText.Algorithm;

namespace IronText.Automata.Lalr1
{
    public class DotTransition
    {
        public readonly int Token;
        public readonly DotState To;

        public DotTransition(int token, DotState to)
        {
            this.Token = token;
            this.To = to;
        }
    }
}
