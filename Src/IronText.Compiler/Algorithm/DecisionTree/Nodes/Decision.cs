using System.Text;

namespace IronText.Algorithm
{
    public abstract class Decision
    {
        public int? Label;

        public abstract int Decide(int value);

        public abstract void Accept(IDecisionVisitor program);

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            var writer = new DecisionProgramWriter(output);
            writer.Build(this, new ActionDecision(int.MinValue));
            return output.ToString();
        }
    }
}
