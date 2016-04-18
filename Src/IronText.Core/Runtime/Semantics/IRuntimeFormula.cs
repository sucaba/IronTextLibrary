namespace IronText.Runtime.Semantics
{
    public interface IRuntimeFormula
    {
        void Execute(IStackLookback<ActionNode> lookback);
        void Execute(IStackLookback<ActionNode> lookback, ActionNode outcomeNode);
    }
}