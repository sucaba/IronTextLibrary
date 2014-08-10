using IronText.Reflection;

namespace IronText.Runtime
{
    struct Reduction<T>
    {
        public readonly GssNode<T> RightNode;
        public readonly Production Rule;
        public readonly int        Token;
        public readonly int        Size;
        public readonly GssLink<T> RightLink;

        public Reduction(GssNode<T> rightNode, Production prod, GssLink<T> rightLink)
        {
            this.Rule           = prod;
            this.RightNode      = rightNode;
            this.Token          = prod.OutcomeToken;
            this.Size           = prod.InputLength;
            this.RightLink      = rightLink;
        }
    }
}
