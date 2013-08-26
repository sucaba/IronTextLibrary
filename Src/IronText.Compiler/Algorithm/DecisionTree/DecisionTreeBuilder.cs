using System.Collections.Generic;
using System.Linq;

namespace IronText.Algorithm
{
    public class DecisionTreeBuilder
    {
        public int MaxLinearCount = 3;
        public const double SwitchCost = 7;
        public const double BranchCost = 3;
        public const int SwitchElementCountThreashold = 1024;
        public const double SwitchDensityThreashold = 0.5;

        private readonly int defaultAction;

        public DecisionTreeBuilder(int defaultAction)
        {
            this.defaultAction = defaultAction;
        }

        public Decision BuildBalanced(
            IIntMap<int>  outcomeArrows,
            IntInterval   possibleBounds,
            IIntFrequency frequency)
        {
            // TODO: Alternatively to averaging probability, each test can 
            // be split if probability varies significantly within test range
            DecisionTest[] tests
                = outcomeArrows
                .EnumerateCoverage(possibleBounds)
                .Select(
                    a => new DecisionTest(a.Key, a.Value, frequency.Average(a.Key))
                    )
                    .ToArray();

            return GenTree(new ArraySlice<DecisionTest>(tests));
        }

        private Decision GenTree(ArraySlice<DecisionTest> S)
        {
            if (S.Count == 1)
            {
                return new ActionDecision(S.ElementAt(0).Action);
            }

            Normalize(S);
            if (IsSwitchable(S) && EntropyOfSwitch(S) > EntropyOfCond(S))
            {
                return GenSwitch(S);
            }
            else
            {
                return GenCond(S);
            }
        }

        private Decision GenCond(ArraySlice<DecisionTest> S)
        {
            double probability;
            int splitIndex = Split(S, out probability);

            int splitElement = S.ElementAt(splitIndex).Interval.First;
            RelationalBranchDecision result;
            RelationalOperator op;
            if (splitIndex == 1 && S.ElementAt(0).Interval.LongSize == 1)
            {
                op = RelationalOperator.Equal;
                result = new RelationalBranchDecision(op, S.ElementAt(0).Interval.First);
            }
            else if (splitIndex == (S.Count - 1) && S.ElementAt(splitIndex).Interval.Size == 1)
            {
                op = RelationalOperator.NotEqual;
                result = new RelationalBranchDecision(op, splitElement);
            }
            else
            {
                op = RelationalOperator.Less;
                result = new RelationalBranchDecision(op, splitElement);
            }

            result.Left  = GenTree(S.SubSlice(0, splitIndex));
            result.Right = GenTree(S.SubSlice(splitIndex));
            return result;
        }

        private Decision GenSwitch(ArraySlice<DecisionTest> S)
        {
            var result = new JumpTableDecision(S);
            return result;
        }

        private double EntropyOfCond(ArraySlice<DecisionTest> S)
        {
            double p1;
            int index = Split(S, out p1);
            double p2 = 1.0 - p1;

            double result = (Functions.Entropy(p1) + Functions.Entropy(1 - p1)) / BranchCost;
            return result;
        }

        private double EntropyOfSwitch(ArraySlice<DecisionTest> S)
        {
            double result = 0;

            foreach (var test in S)
            {
                result += Functions.Entropy(test.Probability);
            }

            result /= SwitchCost;
            return result;
        }

        private int Split(ArraySlice<DecisionTest> S, out double probability)
        {
            var tests = S.Array;

            probability = 0;
            int i = S.Offset;
            int end = S.Offset + S.Count;
            for (; i != end; ++i)
            {
                probability += tests[i].Probability;
                if (probability > 0.5)
                {
                    break;
                }
            }

            int result = i + 1 - S.Offset; // return relative index
            if (result == S.Count)
            {
                probability -= tests[i].Probability;
                --result;
            }

            return result;
        }

        private bool IsSwitchable(ArraySlice<DecisionTest> S)
        {
            long elementCount = S.Sum(test => test.Interval.LongSize);
            if (elementCount > SwitchElementCountThreashold)
            {
                return false;
            }

            var rangeSize = S.ElementAt(S.Count - 1).Interval.Last 
                          - S.ElementAt(0).Interval.First 
                          + 1;

            double density = elementCount / (double)rangeSize;
            return density > SwitchDensityThreashold;
        }

        private void Normalize(ArraySlice<DecisionTest> S)
        {
            var tests = S.Array;

            double sum = 0;
            foreach (var test in S)
            {
                sum += test.Probability;
            }

            foreach (var test in S)
            {
                test.Probability /= sum;
            }
        }

        public Decision BuildBinaryTree(IntArrow<int>[] intArrows)
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
            if (size > MaxLinearCount)
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
