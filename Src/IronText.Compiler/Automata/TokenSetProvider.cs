using IronText.Algorithm;

namespace IronText.Automata
{
    class TokenSetProvider
    {
        public TokenSetProvider(IBuildtimeGrammar grammar)
        {
            this.TokenSet = new BitSetType(grammar.SymbolCount);
        }

        public BitSetType TokenSet { get; }
    }
}
