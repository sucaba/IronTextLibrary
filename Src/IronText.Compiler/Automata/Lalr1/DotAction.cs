using System;

namespace IronText.Automata.Lalr1
{
    abstract class DotAction : IEquatable<DotAction>
    {
        public DotAction(int lookaheadToken)
        {
            this.LookeaheadToken = lookaheadToken;
        }

        public int LookeaheadToken { get; private set; }

        public virtual bool Equals(DotAction other) =>
            LookeaheadToken == other?.LookeaheadToken;

        public override bool Equals(object obj) =>
            Equals(obj as DotAction);

        public override int GetHashCode() => LookeaheadToken;
    }

    class DotGotoAction : DotAction
    {
        public DotGotoAction(int token)
            : base(lookaheadToken: token)
        {
        }

        public int Token => LookeaheadToken;

        public override bool Equals(DotAction other) =>
            Token == (other as DotGotoAction)?.Token;

        public override int GetHashCode() => Token;
    }

    class DotReduceAction : DotAction
    {
        public DotReduceAction(int productionId, int lookahead)
            : base(lookaheadToken: lookahead)
        {
            this.ProductionId = productionId;
        }

        public int ProductionId { get; }

        public override bool Equals(DotAction other) =>
            ProductionId == (other as DotReduceAction)?.ProductionId;

        public override int GetHashCode() => ProductionId;
    }
}
