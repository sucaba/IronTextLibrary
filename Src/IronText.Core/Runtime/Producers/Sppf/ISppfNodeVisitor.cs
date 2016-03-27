using IronText.Logging;

namespace IronText.Runtime
{
    public interface ISppfNodeVisitor
    {
        void VisitLeaf(int matcherIndex, string text, HLoc location);

        void VisitBranch(int productionIndex, SppfNode[] children, HLoc location);

        void VisitAlternatives(SppfNode alternatives);
    }

    public interface ISppfNodeVisitor<T>
    {
        T VisitLeaf(int matcherIndex, string text, HLoc location);

        T VisitBranch(int productionIndex, SppfNode[] children, HLoc location);

        T VisitAlternatives(SppfNode alternatives);
    }
}
