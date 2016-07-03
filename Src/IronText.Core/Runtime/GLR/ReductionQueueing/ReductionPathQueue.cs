using System.Collections.Generic;

namespace IronText.Runtime
{
    sealed class ReductionPathQueue<T> : IReductionQueue<T>
    {
        private readonly Queue<GssReducePath<T>> paths = new Queue<GssReducePath<T>>(10);

        public bool IsEmpty => paths.Count == 0;

        public void Enqueue(
            GssLink<T>        rightLink,
            RuntimeProduction production)
        {
            GssReducePath<T>.ForEach(
                production,
                rightLink.LeftNode,
                rightLink,
                paths.Enqueue);
        }

        public void Enqueue(
            GssNode<T>        rightNode,
            RuntimeProduction production)
        {
            GssReducePath<T>.ForEach(
                production,
                rightNode,
                null,
                paths.Enqueue);
        }

        public GssReducePath<T> Dequeue()
        {
            return paths.Dequeue();
        }
    }
}
