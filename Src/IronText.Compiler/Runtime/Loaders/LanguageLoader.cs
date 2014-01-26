using IronText.Build;
using IronText.MetadataCompiler;

namespace IronText.Runtime
{
    public class LanguageLoader : ILanguageLoader
    {
        public ILanguage Load(LanguageName languageName)
        {
            var provider = new NamedLanguageProvider(languageName);
            ILanguage result;
            ResourceContext.Instance.LoadOrBuild(provider, out result);
            return result;
        }
    }
}
