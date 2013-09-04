using IronText.Algorithm;

namespace IronText.Automata.Regular
{
    public interface IRegularAlphabet
    {
        IntSetType SymbolSetType { get; }

        int EoiSymbol { get; }

        IntSet Symbols { get; }

        int Encode(int input);

        IntSet Encode(IntSet inputSet);

        IntSet Decode(int symbol);

        IntSet Decode(IntSet symbols);
    }
}
