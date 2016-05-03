namespace IronText.Runtime
{
    struct Reduction<T>
    {
        public readonly RuntimeProduction Production;
        public readonly GssNode<T>        RightNode;
        public readonly GssLink<T>        RightLink;
        public readonly int               Token;
        public readonly int               Size;

        public Reduction(
            GssNode<T>        rightNode,
            RuntimeProduction production,
            GssLink<T>        rightLink)
        {
            this.Production = production;
            this.RightNode  = rightNode;
            this.Token      = production.Outcome;
            this.Size       = production.InputLength;
            this.RightLink  = rightLink;
        }
    }
}
