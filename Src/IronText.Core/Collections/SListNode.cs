
namespace IronText.Collections
{
    public class SListNode<TNode> where TNode : SListNode<TNode>
    {
        public TNode Next  { get; internal set; }

        public TNode SetNext(TNode next)
        {
            this.Next = next;
            return (TNode)this;
        }
    }
}
