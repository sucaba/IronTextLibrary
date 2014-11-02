using IronText.Logging;

namespace IronText.Reflection
{
    public interface IGrammarReader
    {
        Grammar Read(IGrammarSource source, ILogging logging);
    }
}
