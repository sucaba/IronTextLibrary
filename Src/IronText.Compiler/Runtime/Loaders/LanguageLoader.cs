using IronText.Build;
using IronText.MetadataCompiler;
using IronText.Reflection;

namespace IronText.Runtime
{
    public class LanguageLoader : ILanguageLoader
    {
        public ILanguageRuntime Load(IGrammarSource source)
        {
            var provider = new NamedLanguageProvider(source);
            ILanguageRuntime result;
            ResourceContext.Instance.LoadOrBuild(provider, out result);
            return result;
        }
    }
}
