﻿using IronText.Build;
using IronText.MetadataCompiler;

namespace IronText.Runtime
{
    public class LanguageLoader : ILanguageLoader
    {
        public ILanguageRuntime Load(ILanguageSource source)
        {
            var provider = new NamedLanguageProvider(source);
            ILanguageRuntime result;
            ResourceContext.Instance.LoadOrBuild(provider, out result);
            return result;
        }
    }
}
