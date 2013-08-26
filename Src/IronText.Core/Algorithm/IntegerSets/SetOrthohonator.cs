using System.Collections.Generic;

namespace IronText.Algorithm
{
    internal class SetOrthohonator
    {
        /// <summary>
        /// Input
        ///  - list of sets L0
        /// Output:
        ///  - list of sets L
        ///  - element index -> set index
        /// Ensure:
        ///  - for any S0, S1 in L : S0*S1 = {}
        ///  - for any S in L: exists S0 in L0: S.issubsetof(S0) // this ensures that we only splitted sets and each set has single regex position
        ///  - not exist S in L: S = {}
        ///  - union(all in L) = union(all in L0)
        ///  - size(L) is minimal amont all possible L
        /// </summary>
        /// <param name="sets"></param>
        public static void Orthohonate(List<IntSet> sets)
        {
            for (int i = 0; i != sets.Count; ++i)
            {
                IntSet x = sets[i];
                if (x.Count == 0)
                {
                    continue;
                }
                
                for (int j = i + 1; j != sets.Count; ++j)
                {
                    var y = sets[j];
                    if (y.Count == 0)
                    {
                        continue;
                    }

                    IntSet common, xOnly, yOnly;
                    if (x.Rel(y, out xOnly, out common, out yOnly))
                    {
                        x = sets[i] = xOnly;
                        sets[j] = yOnly;
                        sets.Insert(i + 1, common);
                        ++j;
                    }
                }
            }

            sets.RemoveAll(s => s.Count == 0);
        }

        public static void OrthohonalAdd(List<IntSet> sets, IntSet x)
        {
            if (x.IsEmpty || sets.IndexOf(x) >= 0)
            {
                return;
            }

            int i = sets.Count;
            sets.Add(x);
            OrthohonateAt(sets, x, i);
        }

        private static void OrthohonateAt(List<IntSet> sets, IntSet x, int xIndex)
        {
            for (int j = 0; j != sets.Count; ++j)
            {
                if (j == xIndex)
                {
                    continue;
                }

                var y = sets[j];
                if (y.Count == 0)
                {
                    continue;
                }

                IntSet common, xOnly, yOnly;
                if (x.Rel(y, out xOnly, out common, out yOnly))
                {
                    x = sets[xIndex] = xOnly;
                    sets[j] = yOnly;
                    sets.Insert(xIndex + 1, common);
                    ++j;
                }
            }
        }
    }
}
