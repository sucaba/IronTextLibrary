using System;

namespace IronText.Automata.TurnPlanning
{
    public abstract class Turn : IEquatable<Turn>
    {
        public virtual int? TokenToConsume => null;

        public bool IsConsuming => TokenToConsume.HasValue;

        public static Turn TopDownReduction(int productionId) =>
            new TopDownReductionTurn(productionId);

        public static Turn BottomUpReduction(int productionId) =>
            new BottomUpReductionTurn(productionId);

        public static Turn Enter(int token) =>
            new EnterTurn(token);

        public static Turn Return(int token) =>
            new ReturnTurn(token);

        public static Turn InputConsumption(int token) =>
            new InputConsumptionTurn(token);

        public static Turn Acceptance() =>
            new AcceptanceTurn();

        public static Turn Unknown() => UnknownTurn.Instance;

        public static bool operator ==(Turn x, Turn y) => x.Equals(y);
        public static bool operator !=(Turn x, Turn y) => !(x == y);

        public abstract bool Equals(Turn other);

        public override bool Equals(object obj) =>
            Equals(obj as Turn);

        public override int GetHashCode() => base.GetHashCode();

        public bool Consumes(int token) => TokenToConsume == token;
    }
}
