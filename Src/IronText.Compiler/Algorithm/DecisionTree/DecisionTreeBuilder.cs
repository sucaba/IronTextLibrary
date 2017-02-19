using System.Collections.Generic;
using System.Linq;

namespace IronText.Algorithm
{
    public class DecisionTreeBuilder
    {
        private int defaultAction;
        private readonly DecisionTreePlatformInfo platform;
        private readonly Dictionary<int,ActionDecision> actionIdToDecision = new Dictionary<int,ActionDecision>();

        public DecisionTreeBuilder(DecisionTreePlatformInfo platform)
        {
            this.platform = platform;
        }

        public ICollection<ActionDecision> ActionDecisions { get { return actionIdToDecision.Values; } }

        public ActionDecision DefaultActionDecision { get { return actionIdToDecision[defaultAction]; } }

        public Decision Build(
            IIntMap<int>  outcomeArrows,
            IntInterval   possibleBounds,
            IIntFrequency frequency)
        {
            defaultAction = outcomeArrows.DefaultValue;
            this.actionIdToDecision[defaultAction] = new ActionDecision(defaultAction);

            Decision result;

            if (TryBuildElementaryTree(outcomeArrows, out result))
            {
                return result;
            }

            // TODO: Alternatively to averaging probability, each test can 
            // be split if probability varies significantly within test range
            DecisionTest[] tests
                = outcomeArrows
                .EnumerateCoverage(possibleBounds)
                .Select(
                    a => new DecisionTest(a.Key, a.Value, frequency.Average(a.Key))
                    )
                    .ToArray();

            result = GenTree(new ArraySlice<DecisionTest>(tests));
            return result;
        }

        private bool TryBuildElementaryTree(IIntMap<int> map, out Decision result)
        {
            result = null;

            int count = 0;
            foreach (var arrow in map.Enumerate())
            {
                if (arrow.Value == map.DefaultValue)
                {
                    continue;
                }

                ++count;
                if (count > platform.MaxLinearCount || arrow.Key.LongSize != 1)
                {
                    return false;
                }
            }

            RelationalBranchDecision lastParent = null;
            foreach (var arrow in map.Enumerate())
            {
                if (arrow.Value == map.DefaultValue)
                {
                    continue;
                }

                var node = new RelationalBranchDecision(RelationalOperator.Equal, arrow.Key.First);
                node.Left = GetActionDecision(arrow.Value);
                if (lastParent == null)
                {
                    result = node;
                }
                else
                {
                    lastParent.Right = node;
                }

                lastParent = node;
            }

            if (lastParent == null)
            {
                result = DefaultActionDecision;
            }
            else
            {
                lastParent.Right = DefaultActionDecision;
            }

            return true;
        }

        private Decision GenTree(ArraySlice<DecisionTest> S)
        {
            if (S.Count == 1)
            {
                return GetActionDecision(S[0].Action);
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

        private ActionDecision GetActionDecision(int id)
        {
            ActionDecision result;
            if (!actionIdToDecision.TryGetValue(id, out result))
            {
                result = new ActionDecision(id);
                actionIdToDecision[id] = result;
            }

            return result;
        }

        private Decision GenCond(ArraySlice<DecisionTest> S)
        {
            double probability;
            int splitIndex = Split(S, out probability);

            int splitElement = S[splitIndex].Interval.First;
            RelationalBranchDecision result;
            RelationalOperator op;
            if (splitIndex == 1 && S[0].Interval.LongSize == 1)
            {
                op = RelationalOperator.Equal;
                result = new RelationalBranchDecision(op, S[0].Interval.First);
            }
            else if (splitIndex == (S.Count - 1) && S[splitIndex].Interval.LongSize == 1)
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
            var result = new JumpTableDecision(S, GetActionDecision);
            return result;
        }

        private double EntropyOfCond(ArraySlice<DecisionTest> S)
        {
            double p1;
            int index = Split(S, out p1);
            double p2 = 1.0 - p1;

            double result = (Functions.Entropy(p1) + Functions.Entropy(1 - p1)) / platform.BranchCost;
            return result;
        }

        private double EntropyOfSwitch(ArraySlice<DecisionTest> S)
        {
            double result = 0;

            foreach (var test in S)
            {
                result += Functions.Entropy(test.Probability);
            }

            result /= platform.SwitchCost;
            return result;
        }

        private int Split(ArraySlice<DecisionTest> S, out double probability)
        {
            probability = 0;
            int i = 0;
            int end = S.Count;
            for (; i != end; ++i)
            {
                probability += S[i].Probability;
                if (probability > 0.5)
                {
                    break;
                }
            }

            int result = i + 1; // return relative index
            if (result == S.Count)
            {
                probability -= S[i].Probability;
                --result;
            }

            return result;
        }

        private bool IsSwitchable(ArraySlice<DecisionTest> S)
        {
            long elementCount = S.Sum(test => test.Interval.LongSize);
            if (elementCount > platform.MaxSwitchElementCount)
            {
                return false;
            }

            var rangeSize = S[S.Count - 1].Interval.Last 
                          - S[0].Interval.First 
                          + 1;

            double density = elementCount / (double)rangeSize;
            return density > platform.MinSwitchDensity;
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
    }
}
