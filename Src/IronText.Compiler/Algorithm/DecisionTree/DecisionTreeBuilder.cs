using System.Collections.Generic;
using System.Linq;

namespace IronText.Algorithm
{
    public class DecisionTreeBuilder
    {
        private readonly int defaultAction;
        private readonly DecisionTreePlatformInfo platform;
        private readonly Dictionary<int,ActionDecision> actionIdToDecision = new Dictionary<int,ActionDecision>();

        public DecisionTreeBuilder(int defaultAction, DecisionTreePlatformInfo platform)
        {
            this.defaultAction = defaultAction;
            this.actionIdToDecision[defaultAction] = new ActionDecision(defaultAction);
            this.platform = platform;
        }

        public ICollection<ActionDecision> ActionDecisions { get { return actionIdToDecision.Values; } }

        public ActionDecision DefaultActionDecision { get { return actionIdToDecision[defaultAction]; } }

        public Decision Build(
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
                return GetActionDecision(S.ElementAt(0).Action);
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
            if (elementCount > platform.MaxSwitchElementCount)
            {
                return false;
            }

            var rangeSize = S.ElementAt(S.Count - 1).Interval.Last 
                          - S.ElementAt(0).Interval.First 
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
