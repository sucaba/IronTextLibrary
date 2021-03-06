﻿
namespace IronText.Runtime
{
    sealed class GssLink<T>
    {
        public readonly GssNode<T> LeftNode;
        public readonly GssLink<T> NextLink;

        private T label;

        public GssLink(GssNode<T> leftNode, T label, GssLink<T> nextSibling = null)
        {
            this.LeftNode    = leftNode;
            this.label       = label;
            this.NextLink = nextSibling;
        }

        public T Label { get { return label; } }

        public void AssignLabel(T label)
        {
            this.label = label;
        }
    }
}
