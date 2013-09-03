
namespace IronText.Framework
{
    interface IReductionQueue<TNode>
    {
        bool IsEmpty { get; }

        GssReducePath<TNode> Dequeue();

        void Enqueue(GssNode<TNode> tailNode, BnfRule rule, int size);

        void Enqueue(GssLink<TNode> tailLink, BnfRule rule, int size);
    }
}
