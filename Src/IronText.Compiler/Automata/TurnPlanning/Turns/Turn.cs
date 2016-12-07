using System;

namespace IronText.Automata.TurnPlanning
{
    public abstract class Turn : IEquatable<Turn>
    {
        public virtual int? TokenToConsume => null;

        public static Turn InnerReduction(int productionId) =>
            new InnerReductionTurn(productionId);

        public static Turn Return(int token) =>
            new ReturnTurn(token);

        public static Turn InputConsumption(int token) =>
            new InputConsumptionTurn(token);
        public static Turn Acceptance() =>
            new AcceptanceTurn();

        public static bool operator ==(Turn x, Turn y) => x.Equals(y);
        public static bool operator !=(Turn x, Turn y) => !(x == y);

        public abstract bool Equals(Turn other);

        public override bool Equals(object obj) =>
            Equals(obj as Turn);

        public override int GetHashCode() => base.GetHashCode();

        public virtual bool Consumes(int token) => false;
    }
}
