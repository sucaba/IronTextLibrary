namespace IronText.Automata.TurnPlanning
{
    class TopDownReductionTurn : ReductionTurnBase
    {
        public TopDownReductionTurn(int productionId)
            : base(productionId)
        {
        }

        public override bool Equals(Turn other)
        {
            var similar = other as TopDownReductionTurn;
            return (object)similar != null
                && similar.ProductionId == ProductionId;
        }

        public override int GetHashCode() => ProductionId << 16;

        public override string ToString() => $"LL-reduce-{ProductionId}";
    }
}
