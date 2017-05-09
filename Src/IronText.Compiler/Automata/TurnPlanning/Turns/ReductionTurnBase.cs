using System.Diagnostics;

namespace IronText.Automata.TurnPlanning
{
    abstract class ReductionTurnBase : Turn
    {
        public ReductionTurnBase(int productionId)
        {
            this.ProductionId = productionId;
        }

        public int ProductionId { get; }
    }
}
