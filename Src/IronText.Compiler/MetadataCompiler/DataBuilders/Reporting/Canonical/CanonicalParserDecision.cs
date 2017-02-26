using IronText.Runtime;

namespace IronText.Reflection.Reporting
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
        }

        public string       ActionText { get; }

        public IParserState NextState  { get; }
        
    }
}
