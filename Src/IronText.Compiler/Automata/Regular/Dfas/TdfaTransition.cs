using IronText.Algorithm;

namespace IronText.Automata.Regular
{
    public struct TdfaTransition
    {
        public readonly int From;
        public readonly int To;
        public readonly MutableIntSet Symbols;

        public TdfaTransition(int from, MutableIntSet symbols, int to)
        {
            From = from;
            Symbols = symbols;
            To = to;
        }

        public bool HasSymbol(int c) { return Symbols.Contains(c); }

        public bool HasSingleSymbol(int c) { return Symbols.Count == 1 && Symbols.Contains(c); }
    }
}
