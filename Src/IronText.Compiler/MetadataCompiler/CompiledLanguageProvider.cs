using System;
using System.Collections.ObjectModel;
using System.Reflection;
using IronText.Build;
using IronText.Logging;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    public class CompiledLanguageProvider : IExternalResourceProvider<ILanguageRuntime>
    {
        private readonly TypedLanguageSource languageName;
        private readonly IExternalResourceProvider<Assembly> assemblyProvider;

        public CompiledLanguageProvider(TypedLanguageSource languageName, IExternalResourceProvider<Assembly> assemblyProvider)
        {
            this.languageName = languageName;
            this.assemblyProvider = assemblyProvider;
        }

        public ILanguageRuntime Resource { get; private set; }

        public bool Exists
        {
            get { return assemblyProvider.Exists; }
        }

        public DateTime Timestamp
        {
            get { return assemblyProvider.Timestamp; }
        }

        public ReadOnlyCollection<IExternalResource> Sources
        {
            get { return Array.AsReadOnly(new IExternalResource[] { assemblyProvider }); }
        }

        public bool Rebuild(ILogging logging)
        {
            return Load(logging);
        }

        public bool Load(ILogging logging)
        {
            if (!assemblyProvider.Load(logging))
            {
                return false;
            }

            var type = assemblyProvider.Resource.GetType(languageName.LanguageTypeName);
            if (type == null)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = string.Format(
                                    "Unable to load type '{0}' from the assembly '{1}'",
                                    languageName.LanguageTypeName,
                                    assemblyProvider.Resource.FullName),
                    });
                return false;
            }

            Resource = (ILanguageRuntime)Activator.CreateInstance(type);
            return true;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as CompiledLanguageProvider;
            return casted != null
                && object.Equals(casted.languageName, languageName);
        }

        public override int GetHashCode()
        {
            return languageName.GetHashCode();
        }

        public override string ToString()
        {
            return languageName.LanguageName;
        }
    }
}
