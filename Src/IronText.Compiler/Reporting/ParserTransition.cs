using System.Collections.Generic;
using IronText.Runtime;

namespace IronText.Reporting
{
    class ParserTransition : IParserTransition
    {
        public ParserTransition(int token, IEnumerable<ParserAction> actions)
        {
            this.Token = token;
            this.Actions = actions;
        }

        public int Token { get; private set; }

        public IEnumerable<ParserAction> Actions { get; private set; }
    }
}
