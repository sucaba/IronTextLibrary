using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Lib.RegularAst;

namespace IronText.Automata.Regular
{
    class FollowPosBuilder : IAstNodeVisitor<object,int>
    {
        private readonly List<RegularPositionInfo> positions;
        private readonly FirstPosGetter firstPosGetter = new FirstPosGetter();
        private readonly LastPosGetter lastPosGetter = new LastPosGetter();

        public FollowPosBuilder(List<RegularPositionInfo> positions)
        {
            this.positions = positions;
        }

        public object Visit(CharSetNode node, int posOffset)
        {
            return this;
        }

        public object Visit(ActionNode node, int posOffset)
        {
            return this;
        }

        public object Visit(CatNode node, int posOffset)
        {
            FillFollowPos(posOffset, positions, node.Children);
            return this;
        }

        public object Visit(OrNode node, int posOffset)
        {
            int offset = posOffset;
            foreach (var child in node.Children)
            {
                child.Accept(this, offset);
                offset += PosCounter.Of(child);
            }

            return this;
        }

        public object Visit(RepeatNode node, int posOffset)
        {
            FillFollowPos(
                posOffset,
                positions,
                Enumerable.Repeat(node.Inner, node.InnerCompilationCount));

            if (node.MaxCount == int.MaxValue)
            {
                FillFollowPosClosure(
                    posOffset + node.MinCount * PosCounter.Of(node.Inner),
                    node.Inner);
            }


            return this;
        }

        public void FillFollowPos(
            int                       posOffset,
            List<RegularPositionInfo> positions,
            IEnumerable<AstNode>      children)
        {
            int leftOffset = posOffset;
            for (int i = 1; i < children.Count(); ++i)
            {
                var left  = children.ElementAt(i - 1);

                int rightOffset = leftOffset + PosCounter.Of(left);

                foreach (int leftPos in lastPosGetter.LastPos(posOffset, children.Take(i)))
                {
                    IntSet firstPos = firstPosGetter.FirstPos(rightOffset, children.Skip(i));
                    positions[leftPos].FollowPos.AddAll(firstPos);
                }

                leftOffset = rightOffset;
            }

            int offset = posOffset;
            foreach (var node in children)
            {
                node.Accept(this, offset);
                offset += PosCounter.Of(node);
            }
        }

        private void FillFollowPosClosure(int posOffset, AstNode inner)
        {
            var firstPosVisitor = new FirstPosGetter();

            foreach (int pos in inner.Accept(lastPosGetter, posOffset))
            {
                positions[pos].FollowPos.AddAll(inner.Accept(firstPosVisitor, posOffset));
            }
        }
    }
}
