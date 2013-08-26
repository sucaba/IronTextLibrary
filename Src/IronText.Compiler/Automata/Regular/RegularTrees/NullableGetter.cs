using System.Linq;
using IronText.Lib.RegularAst;

namespace IronText.Automata.Regular
{
    class NullableGetter : IAstNodeVisitor<bool>
    {
        public static readonly NullableGetter Instance = new NullableGetter();

        private NullableGetter() { }

        public bool Visit(CharSetNode node) { return false; }

        public bool Visit(ActionNode node) { return true; }

        public bool Visit(CatNode node)
        {
            return node.Children.All(child => child.Accept(this));
        }

        public bool Visit(OrNode node)
        {
            return node.Children.Any(child => child.Accept(this));
        }

        public bool Visit(RepeatNode node)
        {
            return node.MinCount == 0 || node.Inner.Accept(this);
        }
    }
}
