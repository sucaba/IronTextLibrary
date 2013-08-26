using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Lib.RegularAst;

namespace IronText.Automata.Regular
{
    public class RegularTree
    {
        public const int EoiChar = int.MaxValue - 1;

        public readonly AstNode   AugmentedRoot;
        internal readonly List<RegularPositionInfo> Positions = new List<RegularPositionInfo>();
        private CharSetNode EoiCharSetNode;
        public readonly int EoiPosition;
        public readonly IntSet FirstPos;

        public RegularTree(AstNode root)
        {
            this.EoiCharSetNode = CharSetNode.Create(EoiChar);
            this.AugmentedRoot = new CatNode(new List<AstNode> { root, EoiCharSetNode });

            var positionBuilder = new PositionBuilder();
            AugmentedRoot.Accept(positionBuilder, null);
            Positions = positionBuilder.Positions;

            EoiPosition = Positions.FindIndex(pos => pos.Characters.Contains(EoiChar));
            Debug.Assert(EoiPosition >= 0);

            var firstPosVisitor = new FirstPosGetter();
            this.FirstPos = AugmentedRoot.Accept(firstPosVisitor, 0);

            var followPosBuilder = new FollowPosBuilder(Positions);
            AugmentedRoot.Accept(followPosBuilder, 0);
        }

        public int PosCount { get { return Positions.Count; } }

        public IntSet GetPosSymbols(int pos) { return Positions[pos].Characters; }

        public int? GetPosAction(int pos) { return Positions[pos].Action; }

        public IntSet GetFollowPos(int pos) { return Positions[pos].FollowPos; }

        public IEnumerable<IntSet> GetEquivalenceCsets()
        {
            var result = Positions.Select(node => node.Characters);

            // Ensure that new line symbol does not belong to any
            // equivalence classes:
            if (result.Any(cset => cset.Contains(UnicodeIntSetType.NewLine)))
            {
                var setType = result.First().SetType;
                var newLine = new [] { setType.Of(UnicodeIntSetType.NewLine) };
                result = result.Union(newLine);
            }

            return result;
        }
    }
}
