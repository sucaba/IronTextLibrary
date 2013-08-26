using System.Collections.Generic;

namespace IronText.Algorithm
{
    class TreeNode<T>
    {
        public TreeNode(T item)
        {
            Item = item;
        }

        public readonly T Item;
        public readonly List<TreeNode<T>> Children = new List<TreeNode<T>>();

        public override string ToString()
        {
            return string.Format("TreeNode({0}) {ChildCount={1}", Item, Children.Count);
        }
    }
}
