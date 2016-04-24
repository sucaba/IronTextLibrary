using IronText.Logging;
using IronText.Reflection;
using IronText.Runtime;
using System;

namespace IronText.MetadataCompiler
{
    class GrammarReaderProvider
    {
        public GrammarReaderProvider(
            ILanguageSource source,
            ILogging        logging)
        {
            var readerType = Type.GetType(source.GrammarReaderTypeName);
            if (readerType == null)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = string.Format(
                                    "Unable to find grammar reader '{0}' for language '{1}'",
                                    source.GrammarReaderTypeName,
                                    source.LanguageName),
                        Origin = source.GrammarOrigin
                    });

                return;
            }

            Reader = (IGrammarReader)Activator.CreateInstance(readerType);
        }

        public IGrammarReader Reader { get; }
    }
}
