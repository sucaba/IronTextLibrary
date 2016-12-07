using IronText.Automata;
using IronText.Reflection;

namespace IronText.MetadataCompiler.Analysis
{
    public static class ProductionExtensions
    {
        public static BuildtimeProduction ToBuildtime(this Production production) =>
            new BuildtimeProduction(
                production.Index,
                production.Outcome.Index,
                production.InputTokens);
    }
}
