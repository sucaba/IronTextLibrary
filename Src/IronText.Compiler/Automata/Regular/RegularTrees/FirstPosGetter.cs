using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Lib.RegularAst;

namespace IronText.Automata.Regular
{
    class FirstPosGetter : IAstNodeVisitor<IntSet,int>
    {
        public IntSet Visit(CharSetNode node, int posOffset)
        {
            return SparseIntSetType.Instance.Of(posOffset);
        }

        public IntSet Visit(ActionNode node, int posOffset)
        {
            return SparseIntSetType.Instance.Of(posOffset);
        }

        public IntSet Visit(CatNode node, int posOffset)
        {
            return FirstPos(posOffset, node.Children);
        }

        public IntSet Visit(OrNode node, int posOffset)
        {
            var result = SparseIntSetType.Instance.Mutable();
            int offset = posOffset;
            foreach (var child in node.Children)
            {
                result.AddAll(child.Accept(this, offset));
                offset += PosCounter.Of(child);
            }

            return result.CompleteAndDestroy();
        }

        public IntSet Visit(RepeatNode node, int posOffset)
        {
            return node.Inner.Accept(this, posOffset);
        }

        public IntSet FirstPos(int posOffset, IEnumerable<AstNode> seq)
        {
            var result = SparseIntSetType.Instance.Mutable();
            int offset = posOffset;
            foreach (var node in seq)
            {
                result.AddAll(node.Accept(this, offset));
                if (!node.Accept(NullableGetter.Instance))
                {
                    break;
                }

                offset += PosCounter.Of(node);
            }

            return result.CompleteAndDestroy();
        }
    }
}
