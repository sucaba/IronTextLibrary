using System.Collections.Generic;

namespace IronText.Algorithm
{
    public class InlineFirstDTStrategy : IDecisionTreeGenerationStrategy
    {
        private int generationIndex = 0;
        private readonly List<Decision> knownDecisions = new List<Decision>();
        private readonly IDecisionVisitor visitor;

        public InlineFirstDTStrategy(IDecisionVisitor visitor)
        {
            this.visitor = visitor;
        }

        public void PlanCode(Decision decision)
        {
            if (!knownDecisions.Contains(decision))
            {
                knownDecisions.Add(decision);
            }
        }

        public bool TryInlineCode(Decision decision)
        {
            int index = knownDecisions.IndexOf(decision);
            if (index < 0)
            {
                knownDecisions.Insert(generationIndex++, decision);
                decision.Accept(visitor);
                return true;
            }
            else if (index >= generationIndex)
            {
                if (index != generationIndex)
                {
                    // Swap next planned element and just generated element
                    knownDecisions[index] = knownDecisions[generationIndex];
                    knownDecisions[generationIndex] = decision;
                }

                ++generationIndex;
                decision.Accept(visitor);
                return true;
            }

            return false;
        }

        public void IntermediateGenerateCode()
        {
            GenerateCode();
        }

        public void GenerateCode()
        {
            for (; generationIndex != knownDecisions.Count; )
            {
                var decision = knownDecisions[generationIndex];
                ++generationIndex;
                decision.Accept(visitor);
            }
        }
    }
}
