using System;
using System.Collections.ObjectModel;
using System.Reflection;
using IronText.Framework;
using IronText.Logging;

namespace IronText.Build
{
    public class DelegateProvider<TDelegate> : IExternalResourceProvider<TDelegate> where TDelegate : class
    {
        private readonly IExternalResourceProvider<Assembly> assemblyProvider;
        private readonly string typeName;
        private readonly string methodName;

        public DelegateProvider(IExternalResourceProvider<Assembly> assemblyProvider, string typeName, string methodName)
        {
            this.assemblyProvider = assemblyProvider;
            this.typeName = typeName;
            this.methodName = methodName;
        }

        public TDelegate Resource { get; private set; }

        public bool Exists { get { return Resource != null; } }

        public DateTime Timestamp { get { return DateTime.Now; } }

        public ReadOnlyCollection<IExternalResource> Sources
        {
            get { return new ReadOnlyCollection<IExternalResource>(new IExternalResource[0]); }
        }

        public bool Rebuild(ILogging logging)
        {
            return assemblyProvider.Rebuild(logging) && CreateDelegate(logging);
        }

        public bool Load(ILogging logging)
        {
            return assemblyProvider.Load(logging) && CreateDelegate(logging);
        }

        private bool CreateDelegate(ILogging logging)
        {
            if (Resource == null)
            {
                var type = assemblyProvider.Resource.GetType(typeName);
                if (type == null)
                {
                    return false;
                }

                var method = type.GetMethod(methodName);
                if (method == null)
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Message = string.Format(
                                        "Type '{0}' does not have method '{1}'",
                                        type.FullName,
                                        methodName)
                        });
                    return false;
                }

                Resource = (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), method);
            }

            return true;
        }
    }
}
