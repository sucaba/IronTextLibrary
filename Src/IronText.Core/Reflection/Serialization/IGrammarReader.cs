using System.Collections.Generic;
using IronText.Logging;
using IronText.Reflection.Reporting;

namespace IronText.Reflection
{
    public interface IGrammarReader
    {
        Grammar Read(IGrammarSource source, ILogging logging);
    }
}
