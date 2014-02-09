using System.Collections.Generic;
using IronText.Logging;
using IronText.Reflection.Reporting;

namespace IronText.Reflection
{
    public interface IGrammarBuilder
    {
        Grammar Build(IGrammarSource source, ILogging logging);
    }
}
