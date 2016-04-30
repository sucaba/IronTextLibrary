namespace IronText.Runtime.Semantics
{
    public interface IRuntimeVariable
    {
        void Assign(IStackLookback<ActionNode> lookback, object value);

        void Assign(ActionNode node, object value);
    }
}
