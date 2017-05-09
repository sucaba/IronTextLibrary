namespace IronText.Automata.TurnPlanning
{
    class BottomUpReductionTurn : ReductionTurnBase
    {
        public BottomUpReductionTurn(int productionId)
            : base(productionId)
        {
        }

        public override bool Equals(Turn other)
        {
            var similar = other as BottomUpReductionTurn;
            return (object)similar != null
                && similar.ProductionId == ProductionId;
        }

        public override int GetHashCode() => ProductionId << 16;

        public override string ToString() => $"LR-reduce-{ProductionId}";
    }
}
