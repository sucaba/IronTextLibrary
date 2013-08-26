using System.Collections.Generic;

namespace IronText.Algorithm
{
    public sealed class JumpTableDecision : Decision
    {
        private readonly ArraySlice<DecisionTest> tests;
        private readonly int startElement;
        private readonly ActionDecision[] elementToAction;
        private readonly Dictionary<int, ActionDecision> leafDecisions;

        public JumpTableDecision(ArraySlice<DecisionTest> tests)
        {
            this.tests = tests;
            this.startElement = tests.Array[tests.Offset].Interval.First;
            int elementCount = tests.Array[tests.Offset + tests.Count - 1].Interval.Last - startElement + 1;
            this.elementToAction = new ActionDecision[elementCount];

            this.leafDecisions = new Dictionary<int, ActionDecision>();
            foreach (var test in tests)
            {
                if (!leafDecisions.ContainsKey(test.Action))
                {
                    leafDecisions.Add(test.Action, new ActionDecision(test.Action));
                }

                for (int i = test.Interval.First; i <= test.Interval.Last; ++i)
                {
                    elementToAction[i - startElement] = leafDecisions[test.Action];
                }
            }
        }

        public override int Decide(int value)
        {
            value -= startElement;
            return elementToAction[value].Decide(value);
        }

        public override void PrintProgram(IDecisionProgramWriter program)
        {
            program.JumpTable(this, startElement, elementToAction);
            foreach (var action in leafDecisions.Values)
            {
                action.PrintProgram(program);
            }
        }
    }
}
