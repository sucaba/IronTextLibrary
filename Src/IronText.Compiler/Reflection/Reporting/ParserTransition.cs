using System.Collections.Generic;
using IronText.Runtime;

namespace IronText.Reflection.Reporting
{
    class ParserTransition : IParserTransition
    {
        public ParserTransition(int token, IEnumerable<ParserInstruction> actions)
        {
            this.Token = token;
            this.Actions = actions;
        }

        public int Token { get; private set; }

        public IEnumerable<ParserInstruction> Actions { get; private set; }
    }
}
