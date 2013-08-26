using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Extensibility;

namespace IronText.Automata.Regular
{
    class EquivalenceClassesAlphabet : IRegularAlphabet
    {
        private readonly IntPartition partition;
        private static SparseIntSetType InputSetType = SparseIntSetType.Instance;
        private static SparseIntSetType OuputSetType = SparseIntSetType.Instance;
            // TODO: BitArray will be more suitable because it consumes less memory and is faster 

        public EquivalenceClassesAlphabet(IEnumerable<IntSet> csets)
        {
            var nonEmptycsets = csets.Where(cset => !cset.IsEmpty).ToList();

            this.partition = new IntPartition(nonEmptycsets);
            this.EoiSymbol = partition.GetBlockIndex(RegularTree.EoiChar);
        }

        public void AddInputSet(IntSet cset) { partition.AddSet(cset); }

        public IntSetType SymbolSetType { get { return OuputSetType; } }

        public int EoiSymbol { get; private set; }

        public IntSet Symbols { get { return partition.ValidBlockIndexes; } }

        public int Encode(int ch) { return partition.GetBlockIndex(ch); }

        public IntSet Encode(IntSet cset) { return partition.GetBlockIndexes(cset); }

        public IntSet Decode(int block) { return partition.GetBlock(block); }

        public IntSet Decode(IntSet blocks)
        {
            var result = SymbolSetType.Mutable();
            foreach (int symbol in blocks)
            {
                result.AddAll(Decode(symbol));
            }

            return result;
        }
    }
}
