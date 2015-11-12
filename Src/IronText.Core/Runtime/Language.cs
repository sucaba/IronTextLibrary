using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System;
using System.Collections.Generic;
using System.IO;

namespace IronText.Runtime
{
    /// <summary>
    /// Provides access to language runtime functionality
    /// </summary>
    public static class Language
    {
        private const string DefaultLanguageLoaderTypeName = "IronText.Runtime.LanguageLoader, IronText.Compiler";

        private static readonly Dictionary<IGrammarSource, ILanguageRuntime> languages 
            = new Dictionary<IGrammarSource, ILanguageRuntime>();

        public static ILanguageRuntime Get(Type definitionType)
        {
            return Get(new CilGrammarSource(definitionType));
        }

        public static ILanguageRuntime Get(IGrammarSource source)
        {
            ILanguageRuntime result;
            if (languages.TryGetValue(source, out result))
            {
                return result;
            }
            else
            {
                var loader = GetLoader() ?? new CilExistingLanguageLoader();
                result = loader.Load(source);
                if (result == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Unable to load language '{0}'.",
                            source.LanguageName));
                }

                if (!IsBootstrap(result))
                {
                    languages[source] = result;
                }

                ((IInternalInitializable)result).Init();
            }

            return result;
        }

        private static ILanguageLoader GetLoader()
        {
            var loaderType = Type.GetType(DefaultLanguageLoaderTypeName);
            if (loaderType == null)
            {
                return null;
            }

            var loader = (ILanguageLoader)Activator.CreateInstance(loaderType);
            return loader;
        }

        private static bool IsBootstrap(ILanguageRuntime result)
        {
            return result is IBootstrapLanguage;
        }

        public static TC Parse<TC>(TC context, TextReader input, string document)
            where TC : class
        {
            using (var interp = new Interpreter<TC>(context))
            {
                interp.LoggingKind = LoggingKind.ThrowOnError;
                interp.Parse(input, document);
                return context;
            }
        }

        public static TC Parse<TC>(TC context, string input)
            where TC : class
        {
            using (var interp = new Interpreter<TC>(context))
            {
                interp.LoggingKind = LoggingKind.ThrowOnError;
                interp.Parse(input);
                return context;
            }
        }

        public static TC Parse<TC>(TC context, IEnumerable<Msg> input)
            where TC : class
        {
            using (var interp = new Interpreter<TC>(context))
            {
                interp.LoggingKind = LoggingKind.ThrowOnError;
                interp.Parse(input);
                return context;
            }
        }
    }
}
