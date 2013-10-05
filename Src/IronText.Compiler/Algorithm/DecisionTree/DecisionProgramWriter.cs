using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    public class DecisionProgramWriter : IDecisionVisitor
    {
        private readonly StringBuilder output;
        private readonly IDecisionTreeGenerationStrategy strategy;
        private Decision defaultDecision;
        private int labelGen;

        public DecisionProgramWriter(StringBuilder output)
        {
            this.output = output;
            this.strategy = new InlineFirstDTStrategy(this);
        }

        public void Build(Decision root, Decision defaultDecision)
        {
            this.defaultDecision = defaultDecision;
            strategy.PlanCode(root);
            strategy.GenerateCode();
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

            GenerateCodeOrJump(decision.Left);
            strategy.IntermediateGenerateCode();
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

            // default case:
            GenerateCodeOrJump(defaultDecision);

            foreach (var leaf in decision.LeafDecisions)
            {
                strategy.PlanCode(leaf);
            }

            strategy.IntermediateGenerateCode();
        }

        private void GenerateCodeOrJump(Decision decision)
        {
            if (!strategy.TryInlineCode(decision))
            {
                output.Append("goto ").Append(GetLabelText(decision)).AppendLine();
            }
        }

        private void PutLabel(Decision labelNode)
        {
            output.Append(GetLabelText(labelNode)).Append(": ");
        }

        private int GetLabel(Decision decision)
        {
            if (decision.Label.HasValue)
            {
                return decision.Label.Value;
            }

            strategy.PlanCode(decision);
            decision.Label = labelGen++;
            return decision.Label.Value;
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
