using System.Collections.Generic;

namespace IronText.Reflection
{
    public interface IContextProvider
    {
        IEnumerable<ProductionContext> ProvidedContexts { get; }
    }
}
