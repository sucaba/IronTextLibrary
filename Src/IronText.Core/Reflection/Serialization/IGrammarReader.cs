using IronText.Logging;
using IronText.Runtime;

namespace IronText.Reflection
{
    public interface IGrammarReader
    {
        Grammar Read(ILanguageSource source, ILogging logging);
    }
}
