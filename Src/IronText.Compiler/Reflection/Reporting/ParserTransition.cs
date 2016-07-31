using System.Collections.Generic;
using IronText.Runtime;
using IronText.Automata.Lalr1;

namespace IronText.Reflection.Reporting
{
    class ParserTransition : IParserTransition
    {
        public ParserTransition(int token, ParserDecision decision)
        {
            this.Token     = token;
            this.Decisions = decision;
        }

        public int Token { get; private set; }

        public ParserDecision Decisions { get; private set; }
    }
}
