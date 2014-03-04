using System.Collections.Generic;

namespace IronText.Reflection
{
    public interface IContextProvider
    {
        IEnumerable<ActionContext> ProvidedContexts { get; }
    }
}
