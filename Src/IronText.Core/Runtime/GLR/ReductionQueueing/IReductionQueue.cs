
namespace IronText.Runtime
{
    interface IReductionQueue<TNode>
    {
        bool IsEmpty { get; }

        GssReducePath<TNode> Dequeue();

        void Enqueue(GssNode<TNode> tailNode, RuntimeProduction prod);

        void Enqueue(GssBackLink<TNode> tailLink, RuntimeProduction prod);
    }
}
