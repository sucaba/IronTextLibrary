
namespace IronText.Runtime
{
    sealed class GssBackLink<T>
    {
        public readonly GssNode<T>     PriorNode;
        public readonly GssBackLink<T> NextAlternative;

        public GssBackLink(GssNode<T> priorNode, T label, GssBackLink<T> nextAlternative = null)
        {
            this.PriorNode = priorNode;
            this.Label     = label;
            this.NextAlternative = nextAlternative;
        }

        public T Label { get; private set; }

        public void AssignLabel(T label)
        {
            this.Label = label;
        }
    }
}
