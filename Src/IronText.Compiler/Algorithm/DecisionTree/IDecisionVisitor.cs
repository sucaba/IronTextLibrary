namespace IronText.Algorithm
{
    public interface IDecisionVisitor
    {
        void Visit(ActionDecision decision);

        void Visit(RelationalBranchDecision decision);

        void Visit(JumpTableDecision decision);
    }
}
