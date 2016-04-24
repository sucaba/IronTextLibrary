using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Transformations;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class GrammarProvider
    {
        public GrammarProvider(
            IGrammarReader  reader,
            ILanguageSource source,
            ILogging        logging)
        {
            Grammar = reader.Read(source, logging);
            Grammar.Joint.Add(source);

            new EliminateRightNulls(Grammar).Apply();
            Grammar.BuildIndexes();
        }

        public Grammar Grammar { get; }
    }
}
