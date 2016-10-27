using IronText.Algorithm;
using IronText.Reflection;
namespace IronText.Compiler.Analysis
{
    class TokenSetProvider
    {
        public TokenSetProvider(Grammar grammar)
        {
            this.TokenSet = new BitSetType(grammar.Symbols.Count);
        }

        public BitSetType TokenSet { get; }
    }
}
