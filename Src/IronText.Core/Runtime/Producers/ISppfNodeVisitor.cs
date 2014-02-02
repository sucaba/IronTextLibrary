using IronText.Logging;

namespace IronText.Runtime
{
    public interface ISppfNodeVisitor
    {
        void VisitLeaf(int token, object value, Loc location);

        void VisitBranch(int rule, SppfNode[] children, Loc location);

        void VisitAlternatives(SppfNode alternatives);
    }

    public interface ISppfNodeVisitor<T>
    {
        T VisitLeaf(int token, object value, Loc location);

        T VisitBranch(int rule, SppfNode[] children, Loc location);

        T VisitAlternatives(SppfNode alternatives);
    }
}
