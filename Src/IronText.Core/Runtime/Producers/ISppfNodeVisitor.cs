using IronText.Logging;

namespace IronText.Runtime
{
    public interface ISppfNodeVisitor
    {
        void VisitLeaf(int matcherIndex, object value, Loc location);

        void VisitBranch(int productionIndex, SppfNode[] children, Loc location);

        void VisitAlternatives(SppfNode alternatives);
    }

    public interface ISppfNodeVisitor<T>
    {
        T VisitLeaf(int matcherIndex, object value, Loc location);

        T VisitBranch(int productionIndex, SppfNode[] children, Loc location);

        T VisitAlternatives(SppfNode alternatives);
    }
}
