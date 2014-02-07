using System;
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
            int innerPosCount = PosCounter.Of(node.Inner);

            var result = SparseIntSetType.Instance.Mutable();

            // Traverse optional + last repetitions
            int compilationCount = node.InnerCompilationCount;
            int minCount = Math.Max(node.MinCount, 1);
            for (int i = minCount - 1; i != compilationCount; ++i)
            {
                int offset = posOffset + innerPosCount * i;
                result.AddAll(node.Inner.Accept(this, offset));
            }

            bool isInnerNullable = node.Inner.Accept(NullableGetter.Instance);
            if (isInnerNullable)
            {
                // Traverse forced but nullable repetitions
                for (int i = 0; i != node.MinCount; ++i)
                {
                    int offset = posOffset + innerPosCount * i;
                    result.AddAll(node.Inner.Accept(this, offset));
                }
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
