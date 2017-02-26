using System;
using System.Collections.ObjectModel;
using IronText.Runtime;

namespace IronText.Reporting
{
    class CanonicalParserDecision : IParserDecision
    {
        public CanonicalParserDecision(CanonicalParserAutomata automata, ParserDecision decision)
        {
            var instruction = decision.Instruction;
            switch (instruction.Operation)
            {
                case ParserOperation.Fail:
                    ActionText = "fail";
                    break;
                case ParserOperation.Shift:
                    ActionText = $"shift and go to state {instruction.State}";
                    break;
                case ParserOperation.Reduce:
                    ActionText = $"reduce using rule {instruction.Production}";
                    break;
                case ParserOperation.Accept:
                    ActionText = "accept";
                    break;
                default:
                    ActionText = instruction.Operation.ToString();
                    break;
            }

            if (decision.Instruction.Operation == ParserOperation.Shift)
            {
                NextState = automata.States[decision.Instruction.State];
            }
            else
            {
                NextState = ReductionState.Instance;
            }
        }

        public string       ActionText { get; }

        public IParserState NextState  { get; }

        class ReductionState : IParserState
        {
            public static readonly IParserState Instance = new ReductionState();

            public ReadOnlyCollection<IParserDotItem> DotItems =>
                Array.AsReadOnly(new IParserDotItem[0]);

            public int Index => -1;

            public ReadOnlyCollection<IParserTransition> Transitions =>
                Array.AsReadOnly(new IParserTransition[0]);
        }
    }
}
