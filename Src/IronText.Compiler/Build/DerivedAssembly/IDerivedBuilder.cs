using IronText.Framework;

namespace IronText.Build
{
    public interface IDerivedBuilder<TContext>
    {
        TContext Build(ILogging logging, TContext context);
    }
}
