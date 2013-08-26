using IronText.Algorithm;

namespace IronText.Automata.Regular
{
    internal class RegularPositionInfo
    {
        public IntSet Characters;

        public int? Action;

        public readonly MutableIntSet FollowPos = SparseIntSetType.Instance.Mutable();
    }
}
