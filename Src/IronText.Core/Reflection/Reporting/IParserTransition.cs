using System.Collections.Generic;
using IronText.Runtime;

namespace IronText.Reflection.Reporting
{
    public interface IParserTransition
    {
        int Token { get; }

        IEnumerable<ParserAction> Actions { get; }
    }
}
