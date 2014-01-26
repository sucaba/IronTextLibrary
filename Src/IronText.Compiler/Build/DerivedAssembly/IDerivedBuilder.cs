using IronText.Framework;
using IronText.Logging;

namespace IronText.Build
{
    public interface IDerivedBuilder<TContext>
    {
        TContext Build(ILogging logging, TContext context);
    }
}
