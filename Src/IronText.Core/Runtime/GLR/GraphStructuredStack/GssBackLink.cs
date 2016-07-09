using IronText.Collections;

namespace IronText.Runtime
{
    sealed class GssBackLink<T>
        : Ambiguous<GssBackLink<T>>
    {
        public GssBackLink(
            GssNode<T>     priorNode,
            T              label,
            GssBackLink<T> nextAlternative = null)
            : base(nextAlternative)
        {
            this.PriorNode = priorNode;
            this.Label     = label;
        }

        public GssNode<T> PriorNode { get;  }

        public T          Label     { get; private set; }

        public void AssignLabel(T label) =>
            Label = label;
    }
}
