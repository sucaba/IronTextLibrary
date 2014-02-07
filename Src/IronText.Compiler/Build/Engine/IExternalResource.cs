using System;
using System.Collections.ObjectModel;
using IronText.Logging;

namespace IronText.Build
{
    public interface IExternalResource
    {
        bool Exists { get; }

        DateTime Timestamp { get; }

        ReadOnlyCollection<IExternalResource> Sources { get; }

        bool Rebuild(ILogging logging);

        bool Load(ILogging logging);
    }

    public interface IExternalResourceProvider<T> : IExternalResource where T :  class
    {
        /// <summary>
        /// In-memory representation of the external resource.
        /// </summary>
        T Resource { get; }
    }
}
