using System;

namespace IronText.Algorithm
{
    public sealed class RelationalBranchDecision : BranchDecision
    {
        public RelationalBranchDecision(RelationalOperator op, int operand)
        {
            this.Operator = op;
            this.Operand  = operand;
        }

        public readonly RelationalOperator Operator;
        public readonly int                Operand;

        public override int Decide(int value)
        {
            if (Test(value))
            {
                return Left.Decide(value);
            }

            return Right.Decide(value);
        }

        public override void Accept(IDecisionVisitor program)
        {
            program.Visit(this);
            Left.Accept(program);
            Right.Accept(program);
        }

        private bool Test(int value)
        {
            switch (Operator)
            {
                case RelationalOperator.Equal:          return value == Operand;
                case RelationalOperator.NotEqual:       return value != Operand;
                case RelationalOperator.Less:           return value <  Operand;
                case RelationalOperator.Greater:        return value >  Operand;
                case RelationalOperator.LessOrEqual:    return value <= Operand;
                case RelationalOperator.GreaterOrEqual: return value >= Operand;
            }

            throw new InvalidOperationException("Unsupported relational operator");
        }
    }
}
