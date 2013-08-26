using System.Collections.Generic;
using System.Linq;
using Int = System.Int32;

namespace IronText.Algorithm
{
    /// <summary>
    /// IntSet partition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntPartition
    {
        /// <summary>
        /// Index of the "pseudo-block" (not really in partition) which contains all unknown elements.
        /// </summary>
        public const int OtherSetIndex = 0;

        private readonly int indexStart;
        public readonly List<IntSet> blocks;

        /// <summary>
        /// Creates int sets partition from the sets which cannot be joined.
        /// </summary>
        /// <param name="csets">
        /// Sets of "separate" elements were each set should be subset of some "block" in partition.
        /// </param>
        public IntPartition(List<IntSet> csets)
        {
            this.indexStart = 1;
            this.blocks = csets;
            SetOrthohonator.Orthohonate(csets);
        }

        public void AddSet(IntSet cset)
        {
            SetOrthohonator.OrthohonalAdd(blocks, cset);
        }

        /// <summary>
        /// Mapping from the element to the block index (should be very fast).
        /// </summary>
        /// <param name="element">Set element</param>
        /// <returns></returns>
        public Int GetBlockIndex(Int element)
        {
            int result = blocks.FindIndex(set => set.Contains(element));
            return result < 0 ? OtherSetIndex : result + indexStart;
        }

        public IntSet GetBlockIndexes(IntSet elements)
        {
            var result = SparseIntSetType.Instance.Mutable();
            int count = blocks.Count;
            for (int i = 0; i != count; ++i)
            {
                var cset = blocks[i];
                if (cset.Overlaps(elements))
                {
                    result.Add(i + 1);
                }
            }

            return result.CompleteAndDestroy();
        }

        public IntSet ValidBlockIndexes { get { return SparseIntSetType.Instance.Range(indexStart, blocks.Count - 1); } }

        public IntSet GetBlock(Int block)
        {
            if (block == OtherSetIndex)
            {
                // TODO: Performance
                var others = blocks.Aggregate((x, y) => x.Union(y)).Complement();
                return others;
            }

            return blocks[block - indexStart]; 
        }
    }
}
