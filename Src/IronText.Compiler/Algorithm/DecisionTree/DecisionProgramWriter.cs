using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    public class DecisionProgramWriter : IDecisionVisitor
    {
        private readonly StringBuilder output;
        private int labelGen;

        public DecisionProgramWriter(StringBuilder output)
        {
            this.output = output;
        }

        public void Visit(ActionDecision node)
        {
            PutLabel(node);

            output
                .AppendFormat("action({0})", node.Action)
                .AppendLine();
        }

        public void Visit(RelationalBranchDecision decision)
        {
            PutLabel(decision);

            output
                .AppendFormat(
                    "if (x {0} {1}) goto {2}",
                    decision.Operator.Negate().GetOpeatorText(),
                    decision.Operand,
                    GetLabelText(decision.Right))
                .AppendLine();

            decision.Left.Accept(this);
            decision.Right.Accept(this);
        }

        public void Visit(JumpTableDecision decision)
        {
            PutLabel(decision);

            output
                .AppendFormat(
                    "switch -{0} ({1})",
                    decision.StartElement,
                    string.Join(",", decision.ElementToAction.Select(GetLabelText)))
                .AppendLine();
        }

        private void PutLabel(Decision labelNode)
        {
            int label;
            if (labelNode != null)
            {
                if (labelNode.Label.HasValue)
                {
                    label = labelNode.Label.Value;
                }
                else
                {
                    labelNode.Label = label = labelGen++;
                }
            }
            else
            {
                label = labelGen++;
            }

            output.Append(FormatLabel(label)).Append(": ");
        }

        private int GetLabel(Decision node)
        {
            if (node.Label.HasValue)
            {
                return node.Label.Value;
            }

            node.Label = labelGen++;
            return node.Label.Value;
        }

        private string GetLabelText(Decision node)
        {
            return FormatLabel(GetLabel(node));
        }

        private static string FormatLabel(int label)
        {
            return string.Format("L_{0:D3}", label);
        }
    }
}
