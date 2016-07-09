namespace IronText.Runtime
{
    struct Reduction<T>
    {
        public readonly RuntimeProduction Production;
        public readonly GssNode<T>        RightNode;
        public readonly GssBackLink<T>        RightLink;
        public readonly int               Token;
        public readonly int               Size;

        public Reduction(
            GssNode<T>        rightNode,
            RuntimeProduction production,
            GssBackLink<T>        rightLink)
        {
            this.Production = production;
            this.RightNode  = rightNode;
            this.Token      = production.Outcome;
            this.Size       = production.InputLength;
            this.RightLink  = rightLink;
        }
    }
}
