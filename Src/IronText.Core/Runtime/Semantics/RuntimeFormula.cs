using System;

namespace IronText.Runtime.Semantics
{
    [Serializable]
    public class RuntimeFormula : IRuntimeFormula
    {
        private readonly IRuntimeVariable       lhe;
        private readonly IRuntimeValue[]        arguments;
        private readonly Func<object[], object> body;

        public RuntimeFormula(
            IRuntimeVariable       lhe,
            IRuntimeValue[]        arguments, 
            Func<object[], object> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            this.lhe       = lhe;
            this.arguments = arguments;
            this.body      = body;
        }

        public void Execute(IStackLookback<ActionNode> lookback, ActionNode outcomeNode)
        {
            object result = EvaluateRhs(lookback);
            lhe.Assign(outcomeNode, result);
        }

        public void Execute(IStackLookback<ActionNode> lookback)
        {
            object result = EvaluateRhs(lookback);
            lhe.Assign(lookback, result);
        }

        private object EvaluateRhs(IStackLookback<ActionNode> lookback)
        {
            int count = arguments.Length;
            var args = new object[count];
            for (int i = 0; i != count; ++i)
            {
                args[i] = arguments[i].Eval(lookback);
            }

            var result = body(args);
            return result;
        }
    }
}
