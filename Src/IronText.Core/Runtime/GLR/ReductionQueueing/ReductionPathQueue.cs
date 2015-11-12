using System.Collections.Generic;

namespace IronText.Runtime
{
    using System.Diagnostics;

    sealed class ReductionPathQueue<T> : IReductionQueue<T>
    {
        private readonly LinkedList<GssReducePath<T>> paths = new LinkedList<GssReducePath<T>>();
        private readonly int[] tokenComplexity;

        public ReductionPathQueue(int[] tokenComplexity, RuntimeGrammar grammar)
        {
            this.tokenComplexity = tokenComplexity;

#if false
            Debug.WriteLine("Token complexity order:");

            var tokens = Enumerable
                .Range(0, tokenComplexity.Length)
                .OrderBy(token => tokenComplexity[token])
                ;

            foreach (int token in tokens)
            {
                Debug.WriteLine(grammar.TokenName(token));
            }
#endif
        }

        public bool IsEmpty { get { return paths.Count == 0; } }

        public void Enqueue(GssLink<T> rightLink, RuntimeProduction prod)
        {
            Debug.Assert(rightLink != null);

            int size = prod.InputLength;
            int tail = size == 0 ? 0 : 1;
            GssReducePath<T>.GetAll(
                rightLink.LeftNode,
                size - tail,
                tail,
                prod,
                rightLink,
                InternalEnqueue);
        }

        public void Enqueue(GssNode<T> rightNode, RuntimeProduction prod)
        {
            if (prod.InputLength == 0)
            {
                GssReducePath<T>.GetAll(rightNode, 0, 0, prod, null, InternalEnqueue);
            }
            else
            {
                var link = rightNode.FirstLink;
                
                while (link != null)
                {
                    Enqueue(link, prod);

                    link = link.NextLink;
                }
            }
        }

        private void InternalEnqueue(GssReducePath<T> path)
        {
            int index = paths.Count;
            var node = paths.First;
            while (node != null)
            {
                if (GoesBefore(path, node.Value))
                {
                    paths.AddBefore(node, path);
                    return;
                }

                node = node.Next;
            }

            paths.AddLast(path);
        }

        private bool GoesBefore(GssReducePath<T> x, GssReducePath<T> y)
        {
            int diff = x.LeftNode.Layer - y.LeftNode.Layer;
            if (diff > 0)
            {
                return true;
            }
            else if (diff < 0)
            {
                return false;
            }

            return tokenComplexity[x.Production.OutcomeToken] < tokenComplexity[y.Production.OutcomeToken];
        }

        public GssReducePath<T> Dequeue()
        {
            var result = paths.First.Value;
            paths.RemoveFirst();
            return result;
        }
    }
}
