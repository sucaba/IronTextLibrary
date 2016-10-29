using IronText.Algorithm;
using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Compiler.Analysis
{
    public class DotItem
    {
        private readonly RuntimeProduction production;

        public DotItem(DotItem other)
            : this(other.production, other.Position)
        {
        }

        internal DotItem(RuntimeProduction production, int pos)
        {
            this.production = production;
            this.Position   = pos;
            this.LA         = null;
        }

        public int Position { get; }

        public IEnumerable<int> RemainingInput =>
            production.Input.Skip(Position);

        public IEnumerable<DotItemGotoTransition> GotoTransitions =>
            Position == production.Input.Length
                 ? new DotItemGotoTransition[0] 
                 : new[] { new DotItemGotoTransition(production.Input[Position], this) };

        public IEnumerable<DotItemTransition> AllTransitions
        {
            get
            {
                if (!IsReduce)
                {
                    return GotoTransitions;
                }
                else if (IsAugmented)
                {
                    return new [] {
                        new DotItemAcceptTransition(PredefinedTokens.Eoi)
                    };
                }
                else
                {
                    return LA.Select(
                        token =>
                        new DotItemReduceTransition(token, ProductionId));
                }
            }
        }

        public MutableIntSet LA { get; set; }

        public int PreviousToken =>
            Position == 0
            ? -1
            : production.Input[Position - 1];

        public int Outcome => production.Outcome;

        public bool IsAugmented => PredefinedTokens.AugmentedStart == production.Outcome;

        public bool IsKernel => Position != 0 || IsAugmented;

        public int ProductionId => production.Index;

        public bool IsReduce => Position == production.Input.Length;

        public static bool operator ==(DotItem x, DotItem y) =>
            x.ProductionId == y.ProductionId
            && x.Position == y.Position;

        public static bool operator !=(DotItem x, DotItem y) =>
            !(x == y);

        public override bool Equals(object obj) => this == (DotItem)obj;

        public override int GetHashCode() => unchecked(production.Index + Position);

        public override string ToString() =>
            $"(ProdId={production.Index} Pos={Position} LAs={LA})";

        internal DotItem Goto(int token) =>
            new DotItem(production, Position + 1)
            {
                LA = LA?.EditCopy()
            };
    }
}
