using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    class BinaryDecisionTreeBuilder
    {
        private readonly int defaultAction;
        private readonly DecisionTreePlatformInfo platform;

        public BinaryDecisionTreeBuilder(int defaultAction, DecisionTreePlatformInfo platform)
        {
            this.defaultAction = defaultAction;
            this.platform = platform;
        }

        public Decision Build(IntArrow<int>[] intArrows)
        {
            Decision result = TrySingleKey(intArrows, defaultAction);
            if (result == null)
            {
                var upperBounds = ToUpperBoundsMap(intArrows, defaultAction);
                result = Build(0, upperBounds.Length, upperBounds);
            }

            return result;
        }

        private static Decision TrySingleKey(IntArrow<int>[] intArrows, int defaultValue)
        {
            if (intArrows.Length == 1)
            {
                var interval = intArrows[0].Key;
                if (interval.First == interval.Last)
                {
                    var branch = new RelationalBranchDecision(RelationalOperator.Equal, interval.First);
                    branch.Left = new ActionDecision(intArrows[0].Value);
                    branch.Right = new ActionDecision(defaultValue);
                    return branch;
                }
            }

            return null;
        }

        private Decision Build(
            int first,
            int last,
            KeyValuePair<int,int>[] upperBounds)
        {
            int size = last - first;
            if (size > platform.MaxLinearCount)
            { 
                // binary search nodes
                int middle = first + (size - 1) / 2;

                var branch = new RelationalBranchDecision(
                                    RelationalOperator.LessOrEqual,
                                    upperBounds[middle].Key);

                // positive branch
                branch.Left = Build(first, middle + 1, upperBounds);

                // negative branch
                branch.Right = Build(middle + 1, last, upperBounds);

                return branch;
            }
            else
            {
                Decision result = null;
                RelationalBranchDecision lastBranch = null;

                // linear search nodes
                for (; first != last; ++first)
                {
                    int key = upperBounds[first].Key;

                    RelationalBranchDecision branch;

                    if (first > 0 && (key - upperBounds[first - 1].Key) == 1)
                    {
                        branch = new RelationalBranchDecision(RelationalOperator.Equal, key);
                    }
                    else
                    {
                        branch = new RelationalBranchDecision(RelationalOperator.LessOrEqual, key);
                    }

                    branch.Left = new ActionDecision(upperBounds[first].Value);
                    branch.Right = null;

                    if (lastBranch != null)
                    {
                        lastBranch.Right = branch;
                    }
                    else
                    {
                        result = branch;
                    }

                    lastBranch = branch;
                }

                if (lastBranch != null)
                {
                    lastBranch.Right = new ActionDecision(defaultAction);
                }
                else
                {
                    result = new ActionDecision(defaultAction);
                }

                return result;
            }
        }

        private static KeyValuePair<int, int>[] ToUpperBoundsMap(IntArrow<int>[] intArrows, int defaultValue)
        {
            var resultList = new List<KeyValuePair<int, int>>(intArrows.Length);
            int prev = int.MinValue;
            foreach (var pair in intArrows)
            {
                int first = pair.Key.First;
                int last = pair.Key.Last;

                if (first > (1 + prev))
                // gap between this and prior interval
                {
                    resultList.Add(new KeyValuePair<int, int>(first - 1, defaultValue));

                    resultList.Add(new KeyValuePair<int, int>(last, pair.Value));
                }
                else if (resultList.Count != 0 && resultList[resultList.Count - 1].Value == pair.Value)
                {
                    resultList[resultList.Count - 1] = new KeyValuePair<int, int>(last, pair.Value);
                }
                else
                {
                    resultList.Add(new KeyValuePair<int, int>(last, pair.Value));
                }

                prev = last;
            }

#if false
            Console.WriteLine("Upperbounds:");
            for (int i = 0; i != resultList.Count; ++i)
            {
                var pair = resultList[i];
                Console.WriteLine("{0}: ubound={1}, value={2}", i, pair.Key, pair.Value);
            }
#endif
            return resultList.ToArray();
        }
    }
}
