using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    sealed class InheritanceSortHelper<T>
    {
        private T[] input;
        private Func<T, T, bool> isA;
        private bool reversed;

        public InheritanceSortHelper(T[] input, Func<T, T, bool> isA, bool reversed = false)
        {
            this.input    = input;
            this.isA      = isA;
            this.reversed = reversed;
        }

        public void Sort()
        {
            var forest = BuildForest();
            if (reversed)
            {
                ReversedDepthFirstCopy(forest, input.Length);
            }
            else
            {
                DepthFirstCopy(forest, 0);
            }
        }

        private int DepthFirstCopy(List<TreeNode<T>> forest, int pos)
        {
            foreach (var tree in forest)
            {
                input[pos++] = tree.Item;
                pos = DepthFirstCopy(tree.Children, pos);
            }

            return pos;
        }

        private int ReversedDepthFirstCopy(List<TreeNode<T>> forest, int pos)
        {
            foreach (var tree in forest)
            {
                input[--pos] = tree.Item;
                pos = ReversedDepthFirstCopy(tree.Children, pos);
            }

            return pos;
        }

        public List<TreeNode<T>> BuildForest()
        {
            List<TreeNode<T>> result = new List<TreeNode<T>>();

            foreach (var item in input)
            {
                var node = new TreeNode<T>(item);
                PutNode(node, result);
            }

            return result;
        }

        private void PutNode(TreeNode<T> node, List<TreeNode<T>> forest)
        {
            for (int i = 0; i != forest.Count;)
            {
                var tree = forest[i];
                if (isA(tree.Item, node.Item))
                {
                    node.Children.Add(tree);
                    forest.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }

            bool hasParent = false;
            foreach (var parentCandidateNode in forest)
            {
                if (isA(node.Item, parentCandidateNode.Item))
                {
                    // Note: node can have multiple parents
                    PutNode(node, parentCandidateNode.Children);
                    hasParent = true;
                }
            }

            if (!hasParent)
            {
                forest.Add(node);
            }
        }

        public IEnumerable<Tuple<T, T>> GetInheritancePairs()
        {
            var result = new List<Tuple<T, T>>();

            var forest = BuildForest();
            FillInheritancePairsList(forest, result);

            return result;
        }

        private void FillInheritancePairsList(List<TreeNode<T>> forest, List<Tuple<T, T>> result)
        {
            foreach (var parent in forest)
            {
                foreach (var child in parent.Children)
                {
                    result.Add(Tuple.Create(parent.Item, child.Item));
                }

                FillInheritancePairsList(parent.Children, result);
            }
        }
    }
}
