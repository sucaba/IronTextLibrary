using System.Collections.Generic;
using IronText.Algorithm;

namespace IronText.Automata.Regular
{
    class RegularAlphabet : IRegularAlphabet
    {
        public static readonly IRegularAlphabet Null = new RegularAlphabet(new IntSet[0]);

        private IntSet symbols;

        public RegularAlphabet(IEnumerable<IntSet> csets)
        {
            symbols = SparseIntSetType.Instance.Union(csets);
        }

        public void AddInputSet(IntSet cset) { symbols = symbols.Union(cset); }

        public IntSetType SymbolSetType { get { return SparseIntSetType.Instance; } }

        public int EoiSymbol { get { return RegularTree.EoiChar; } }

        public IntSet Symbols { get { return symbols; } }

        public int Encode(int ch) { return ch; }

        public IntSet Encode(IntSet cset) { return cset; }

        public IntSet Decode(int block) { return SparseIntSetType.Instance.Of(block); }

        public IntSet Decode(IntSet blocks) { return blocks; }
    }
}
