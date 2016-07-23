using IronText.Runtime;
using System.Collections.Generic;

namespace IronText.Reflection.Reporting
{
    public interface IParserTransition
    {
        int Token { get; }

        IEnumerable<ParserInstruction> Actions { get; }
    }
}
