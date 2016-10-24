using IronText.DI;
using IronText.Logging;
using IronText.Runtime;
using System;
using System.ComponentModel;
using System.Reflection;

namespace IronText.MetadataCompiler.DataBuilders.Infrastsructure
{
    class LoggingInstantiator : IInstantiator
    {
        private readonly ILogging logging;
        private readonly ILanguageSource source;

        public LoggingInstantiator(
            ILanguageSource source,
            ILogging        logging)
        {
            this.source  = source; 
            this.logging = logging;
        }

        public object Execute(Type resultType, Func<object> factory)
        {
            object result = null;

            logging.WithTimeLogging(
                source.LanguageName,
                source.GrammarOrigin,
                () => result = factory(),
                GetActionName(resultType));

            return result;
        }

        private static string GetActionName(Type type)
        {
            var result = type.GetCustomAttribute<DescriptionAttribute>()
                            ?.Description
                        ?? type.Name;
            return result;
        }
    }
}
