namespace IronText.Automata
{
    public class TreeNode
    {
        public TreeNode(int outcome)
            : this(-1, outcome, null)
        {
        }

        public TreeNode(
            int productionId,
            int outcome,
            params TreeNode[] components)
        {
            this.ProductionId = productionId;
            this.Outcome      = outcome;
            this.Components   = components;
        }

        public int                       ProductionId { get; }

        public int                       Outcome      { get; }

        public TreeNode[] Components   { get; }
    }
}
