using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IronText.Build;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Misc;

namespace IronText.MetadataCompiler
{
    public class NamedLanguageProvider : IExternalResourceProvider<ILanguage>
    {
        private static readonly Dictionary<LanguageName, ILanguage> bootstrapLanguages = new Dictionary<LanguageName, ILanguage>();

        private readonly LanguageName languageName;
        private IExternalResourceProvider<ILanguage> provider;

        public NamedLanguageProvider(LanguageName languageName)
        {
            this.languageName = languageName;
        }
        
        ILanguage IExternalResourceProvider<ILanguage>.Resource
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

        private IExternalResourceProvider<ILanguage> GetProvider()
        {
            if (provider != null)
            {
                return provider;
            }

            if (Attributes.Exists<DerivedAssemblyMarker>(languageName.SourceAssembly))
            {
                provider = new CompiledLanguageProvider(
                    languageName,
                    new RequiredAssemblyProvider(languageName.SourceAssembly.GetName()));

                return provider;
            }

            provider = new CompiledLanguageProvider(
                languageName,
                new DerivedAssemblyProvider(languageName.SourceAssembly, null));

            if (!ResourceContext.Instance.CanLoadOrBuild(provider))
            {
                provider = new ResourceGetter<ILanguage>(
                    (ILogging logging, out ILanguage output) =>
                    {
                        ILanguage l;
                        if (!bootstrapLanguages.TryGetValue(languageName, out l))
                        {
                            LanguageData data;

                            if (!ResourceContext.Instance.LoadOrBuild(
                                    new LanguageDataProvider(languageName, true),
                                    out data))
                            {
                                output = null;
                                return false;
                            }

                            l = new BootstrapLanguage(languageName, data);
                            bootstrapLanguages[languageName] = l;
                        }

                        output = l;
                        return true;
                    });
            }

            return provider;
        }

        public override string ToString()
        {
            return languageName.Name;
        }
    }
}
