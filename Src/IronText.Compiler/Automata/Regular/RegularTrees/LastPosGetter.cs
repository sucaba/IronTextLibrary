using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Lib.RegularAst;

namespace IronText.Automata.Regular
{
    class LastPosGetter : IAstNodeVisitor<IntSet,int>
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
            return LastPos(posOffset, node.Children);
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

            return result;
        }

        public IntSet Visit(RepeatNode node, int posOffset)
        {
            if (node.Inner.Accept(NullableGetter.Instance) || node.MinCount == 0)
            {
                return LastPos(
                    posOffset,
                    Enumerable.Repeat(node.Inner, node.InnerCompilationCount));
            }

            var result = SparseIntSetType.Instance.Mutable();
            for (int i = node.MinCount - 1; i != node.InnerCompilationCount; ++i)
            {
                int offset = posOffset + PosCounter.Of(node.Inner) * i;
                result.AddAll(node.Inner.Accept(this, offset));
            }

            return result.CompleteAndDestroy();
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

        public IntSet LastPos(int posOffset, IEnumerable<AstNode> seq)
        {
            var result = SparseIntSetType.Instance.Mutable();
            int offset = posOffset + seq.Sum(node => PosCounter.Of(node));
            foreach (var node in Enumerable.Reverse(seq))
            {
                offset -= PosCounter.Of(node);

                result.AddAll(node.Accept(this, offset));
                if (!node.Accept(NullableGetter.Instance))
                {
                    break;
                }
            }

            return result.CompleteAndDestroy();
        }
    }
}
