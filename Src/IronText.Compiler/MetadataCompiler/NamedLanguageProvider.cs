using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IronText.Build;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    public class NamedLanguageProvider : IExternalResourceProvider<ILanguageRuntime>
    {
        private static readonly Dictionary<IGrammarSource, ILanguageRuntime> bootstrapLanguages = new Dictionary<IGrammarSource, ILanguageRuntime>();

        private readonly IGrammarSource languageName;
        private IExternalResourceProvider<ILanguageRuntime> provider;

        public NamedLanguageProvider(IGrammarSource languageName)
        {
            this.languageName = languageName;
        }
        
        ILanguageRuntime IExternalResourceProvider<ILanguageRuntime>.Resource
        {
            get { return GetProvider().Resource; }
        }

        bool IExternalResource.Exists
        {
            get { return GetProvider().Exists; }
        }

        DateTime IExternalResource.Timestamp
        {
            get { return GetProvider().Timestamp; }
        }

        ReadOnlyCollection<IExternalResource> IExternalResource.Sources
        {
            get { return GetProvider().Sources; }
        }

        bool IExternalResource.Rebuild(ILogging logging)
        {
            var provider = GetProvider();
            return provider.Rebuild(logging);
        }

        bool IExternalResource.Load(ILogging logging)
        {
            var provider = GetProvider();
            return provider.Load(logging);
        }

        private IExternalResourceProvider<ILanguageRuntime> GetProvider()
        {
            if (provider != null)
            {
                return provider;
            }

            var cilLanguageName = (CilGrammarSource)this.languageName;

            if (Attributes.Exists<DerivedAssemblyMarker>(cilLanguageName.SourceAssembly))
            {
                provider = new CompiledLanguageProvider(
                    cilLanguageName,
                    new RequiredAssemblyProvider(cilLanguageName.SourceAssembly.GetName()));

                return provider;
            }

            provider = new CompiledLanguageProvider(
                cilLanguageName,
                new DerivedAssemblyProvider(cilLanguageName.SourceAssembly, null, null));

            if (!ResourceContext.Instance.CanLoadOrBuild(provider))
            {
                provider = new ResourceGetter<ILanguageRuntime>(
                    (ILogging logging, out ILanguageRuntime output) =>
                    {
                        ILanguageRuntime l;
                        if (!bootstrapLanguages.TryGetValue(cilLanguageName, out l))
                        {
                            LanguageData data;

                            if (!ResourceContext.Instance.LoadOrBuild(
                                    new LanguageDataProvider(cilLanguageName, true),
                                    out data))
                            {
                                output = null;
                                return false;
                            }

                            l = new BootstrapLanguage(cilLanguageName, data);
                            bootstrapLanguages[cilLanguageName] = l;
                        }

                        output = l;
                        return true;
                    });
            }

            return provider;
        }

        public override string ToString()
        {
            return languageName.LanguageName;
        }
    }
}
