using IronText.Reflection;

namespace IronText.Framework
{
    interface IReductionQueue<TNode>
    {
        bool IsEmpty { get; }

        GssReducePath<TNode> Dequeue();

        void Enqueue(GssNode<TNode> tailNode, Production rule, int size);

        void Enqueue(GssLink<TNode> tailLink, Production rule, int size);
    }
}
