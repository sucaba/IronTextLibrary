using System;

namespace IronText.Algorithm
{
    public enum RelationalOperator
    {
        Equal,
        NotEqual,
        Less,
        Greater,
        LessOrEqual,
        GreaterOrEqual,
    }

    public static class RelationalOperatorExtensions
    {
        public static string GetOpeatorText(this RelationalOperator op)
        {
            switch (op)
            {
                case RelationalOperator.Equal:          return "==";
                case RelationalOperator.NotEqual:       return "!=";
                case RelationalOperator.Less:           return "<";
                case RelationalOperator.Greater:        return ">";
                case RelationalOperator.LessOrEqual:    return "<=";
                case RelationalOperator.GreaterOrEqual: return ">=";
            }

            throw new InvalidOperationException("Not supported operator");
        }

        public static RelationalOperator Negate(this RelationalOperator op)
        {
            switch (op)
            {
                case RelationalOperator.Equal:          return RelationalOperator.NotEqual;
                case RelationalOperator.NotEqual:       return RelationalOperator.Equal;
                case RelationalOperator.Less:           return RelationalOperator.GreaterOrEqual;
                case RelationalOperator.Greater:        return RelationalOperator.LessOrEqual;
                case RelationalOperator.LessOrEqual:    return RelationalOperator.Greater;
                case RelationalOperator.GreaterOrEqual: return RelationalOperator.Less;
            }

            throw new InvalidOperationException("Not supported operand");
        }
    }
}
