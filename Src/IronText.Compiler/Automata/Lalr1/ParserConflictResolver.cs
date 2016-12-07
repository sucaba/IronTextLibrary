using IronText.Reflection;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class ParserConflictResolver
    {
        private readonly IBuildtimeGrammar grammar;

        public ParserConflictResolver(IBuildtimeGrammar grammar)
        {
            this.grammar = grammar;
        }

        public bool TryResolve(
            ParserDecision decisionX,
            ParserDecision decisionY,
            int incomingToken,
            out ParserDecision output)
        {
            ParserInstruction instruction;

            bool result = TryResolveShiftReduce(
                decisionX.Instruction,
                decisionY.Instruction,
                incomingToken,
                out instruction);
            output = new ParserDecision(instruction);

            return result;
        }

        private bool TryResolve(
            ParserInstruction actionX,
            ParserInstruction actionY,
            int incomingToken,
            out ParserInstruction output) =>
            TryResolveShiftReduce(
                actionX,
                actionY,
                incomingToken,
                out output);

        private bool TryResolveShiftReduce(
            ParserInstruction actionX,
            ParserInstruction actionY,
            int incomingToken,
            out ParserInstruction output)
        {
            output = ParserInstruction.FailAction;

            ParserInstruction shiftAction, reduceAction;
            if (actionX.Operation == ParserOperation.Shift
                && actionY.Operation == ParserOperation.Reduce)
            {
                shiftAction = actionX;
                reduceAction = actionY;
            }
            else if (actionY.Operation == ParserOperation.Shift
                && actionX.Operation == ParserOperation.Reduce)
            {
                shiftAction = actionY;
                reduceAction = actionX;
            }
            else
            {
                output = ParserInstruction.FailAction;
                return false;
            }

            var shiftTokenPrecedence = grammar.GetTermPrecedence(incomingToken);
            var reduceRulePrecedence = grammar.GetProductionPrecedence(reduceAction.Production);

            if (shiftTokenPrecedence == null && reduceRulePrecedence == null)
            {
                output = ParserInstruction.FailAction;
                return false;
            }
            else if (shiftTokenPrecedence == null)
            {
                output = reduceAction;
            }
            else if (reduceRulePrecedence == null)
            {
                output = shiftAction;
            }
            else if (Precedence.IsReduce(reduceRulePrecedence, shiftTokenPrecedence))
            {
                output = reduceAction;
            }
            else
            {
                output = shiftAction;
            }

            return true;
        }

    }
}
