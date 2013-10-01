using System.Collections.Generic;

namespace IronText.Framework
{

    sealed class ReductionQueue<T> : IReductionQueue<T>
    {
        private readonly Queue<Reduction<T>> reductions = new Queue<Reduction<T>>(10);
        private readonly Queue<GssReducePath<T>> pendingPaths = new Queue<GssReducePath<T>>(10);

        public ReductionQueue()
        {
        }

        public bool IsEmpty { get { return reductions.Count == 0 && pendingPaths.Count == 0; } }

        public void Enqueue(GssLink<T> rightLink, BnfRule rule, int size)
        {
            reductions.Enqueue(
                new Reduction<T>(
                    rightLink.LeftNode,
                    rule,
                    size,
                    -1,
                    rightLink));
        }

        public void Enqueue(GssNode<T> rightNode, BnfRule rule, int size)
        {
            if (size == 0)
            {
                reductions.Enqueue(
                    new Reduction<T>(
                        rightNode,
                        rule,
                        0,
                        -1,
                        null));
            }
            else
            {
                foreach (var link in rightNode.Links)
                {
                    reductions.Enqueue(
                        new Reduction<T>(
                            link.LeftNode,
                            rule,
                            size,
                            -1,
                            link));
                }
            }
        }

        public GssReducePath<T> Dequeue()
        {
            if (pendingPaths.Count == 0)
            {
                var r = reductions.Dequeue();
                var size = r.Size;

                int tail = (r.Size != 0 && r.RightLink != null) ? 1 : 0;
                GssReducePath<T>.GetAll(
                    r.RightNode,
                    size - tail,
                    tail,
                    r.Rule,
                    r.RightLink,
                    pendingPaths.Enqueue);
            }

            return pendingPaths.Dequeue();
        }
    }
}
