using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    public sealed class JumpTableDecision : Decision
    {
        private readonly ArraySlice<DecisionTest> tests;
        private readonly int startElement;
        private readonly Decision[] elementToAction;
        private readonly List<Decision> leafDecisions = new List<Decision>();

        internal JumpTableDecision(ArraySlice<DecisionTest> tests, Func<int,ActionDecision> idToAction)
        {
            this.tests = tests;
            this.startElement = tests.Array[tests.Offset].Interval.First;
            int elementCount = tests.Array[tests.Offset + tests.Count - 1].Interval.Last - startElement + 1;
            this.elementToAction = new Decision[elementCount];

            foreach (var test in tests)
            {
                var action = idToAction(test.Action);
                if (!leafDecisions.Contains(action))
                {
                    leafDecisions.Add(action);
                }

                for (int i = test.Interval.First; i <= test.Interval.Last; ++i)
                {
                    elementToAction[i - startElement] = action;
                }
            }
        }

        public int StartElement { get { return startElement; } }

        public Decision[] ElementToAction { get { return elementToAction; } }

        public ICollection<Decision> LeafDecisions { get { return leafDecisions; } }

        public override int Decide(int value)
        {
            value -= startElement;
            return elementToAction[value].Decide(value);
        }

        public override void Accept(IDecisionVisitor program)
        {
            program.Visit(this);
        }
    }
}
