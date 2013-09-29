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
            this.Accept(new DecisionProgramWriter(output));
            return output.ToString();
        }
    }
}
