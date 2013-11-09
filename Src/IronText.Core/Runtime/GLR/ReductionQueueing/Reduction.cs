using IronText.Framework.Reflection;

namespace IronText.Framework
{
    using Token = System.Int32;

    struct Reduction<T>
    {
        public readonly GssNode<T> RightNode;
        public readonly Production    Rule;
        public readonly Token      Token;
        public readonly int        Size;
        public readonly int        EpsilonIndex;
        public readonly GssLink<T> RightLink; 

        public Reduction(GssNode<T> rightNode, Production rule, int size, int epsilonIndex, GssLink<T> rightLink)
        {
            this.Rule           = rule;
            this.RightNode      = rightNode;
            this.Token          = rule.Left;
            this.Size           = size;
            this.EpsilonIndex   = epsilonIndex;
            this.RightLink      = rightLink;
        }
    }
}
