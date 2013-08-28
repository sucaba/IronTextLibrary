using IronText.Framework;

namespace IronText.Framework
{
    interface IReductionQueue<TNode>
    {
        bool IsEmpty { get; }

        GssReducePath<TNode> Dequeue();

        void Enqueue(GssNode<TNode> rightNode, BnfRule rule, int size);

        void Enqueue(GssLink<TNode> rightLink, BnfRule rule, int size);
    }
}
