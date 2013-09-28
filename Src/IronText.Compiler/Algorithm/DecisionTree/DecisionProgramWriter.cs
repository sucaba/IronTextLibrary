using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    public class DecisionProgramWriter : IDecisionProgramWriter
    {
        private readonly StringBuilder output;
        private int labelGen;

        public DecisionProgramWriter(StringBuilder output)
        {
            this.output = output;
        }

        public void Action(Decision labelNode, int action)
        {
            PutLabel(labelNode);

            output
                .AppendFormat("action({0})", action)
                .AppendLine();
        }

        public void Jump(Decision labelNode, Decision destination)
        {
            PutLabel(labelNode);

            output
                .AppendFormat("goto {0}", GetLabelText(destination))
                .AppendLine();
        }

        public void CondJump(Decision labelNode, RelationalOperator op, int operand, Decision destination)
        {
            PutLabel(labelNode);

            output
                .AppendFormat(
                    "if (x {0} {1}) goto {2}",
                    op.GetOpeatorText(),
                    operand,
                    GetLabelText(destination))
                .AppendLine();
        }

        public void JumpTable(Decision labelNode, int startElement, Decision[] elementToAction)
        {
            PutLabel(labelNode);

            output
                .AppendFormat(
                    "switch -{0} ({1})",
                    startElement,
                    string.Join(",", elementToAction.Select(GetLabelText)))
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
