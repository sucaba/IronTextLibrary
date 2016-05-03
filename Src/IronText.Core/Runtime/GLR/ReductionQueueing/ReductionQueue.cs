using System.Collections.Generic;

namespace IronText.Runtime
{
    sealed class ReductionQueue<T> : IReductionQueue<T>
    {
        private readonly Queue<Reduction<T>>     reductions    = new Queue<Reduction<T>>(10);
        private readonly Queue<GssReducePath<T>> dequeueBuffer = new Queue<GssReducePath<T>>(10);

        public ReductionQueue()
        {
        }

        public bool IsEmpty => reductions.Count == 0 && dequeueBuffer.Count == 0;

        public void Enqueue(
            GssLink<T>        rightLink,
            RuntimeProduction production)
        {
            reductions.Enqueue(
                new Reduction<T>(
                    rightLink.LeftNode,
                    production,
                    rightLink));
        }

        public void Enqueue(
            GssNode<T>        rightNode,
            RuntimeProduction production)
        {
            if (production.InputLength == 0)
            {
                reductions.Enqueue(
                    new Reduction<T>(rightNode, production, null));
            }
            else
            {
                var link = rightNode.FirstLink;
                while (link != null)
                {
                    reductions.Enqueue(
                        new Reduction<T>(link.LeftNode, production, link));

                    link = link.NextLink;
                }
            }
        }

        public GssReducePath<T> Dequeue()
        {
            if (dequeueBuffer.Count == 0)
            {
                Reduction<T> r = reductions.Dequeue();

                GssReducePath<T>.ForEach(
                    r.Production,
                    r.RightNode,
                    r.RightLink,
                    dequeueBuffer.Enqueue);
            }

            return dequeueBuffer.Dequeue();
        }
    }
}
