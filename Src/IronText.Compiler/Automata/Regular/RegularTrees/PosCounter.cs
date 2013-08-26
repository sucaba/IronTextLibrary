using System.Linq;
using IronText.Lib.RegularAst;

namespace IronText.Automata.Regular
{
    class PosCounter : IAstNodeVisitor<int>
    {
        public static readonly PosCounter Instance = new PosCounter();

        public static int Of(AstNode node) { return node.Accept(Instance); }

        private PosCounter() { }

        public int Visit(CharSetNode node) { return 1; }

        public int Visit(ActionNode node) { return 1; }

        public int Visit(CatNode node) { return node.Children.Sum(child => child.Accept(this)); }

        public int Visit(OrNode node) { return node.Children.Sum(child => child.Accept(this)); }

        public int Visit(RepeatNode node)
        {
            return node.Inner.Accept(this) * node.InnerCompilationCount;
        }
    }
}
