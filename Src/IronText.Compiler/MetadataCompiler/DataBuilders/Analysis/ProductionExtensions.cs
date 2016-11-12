using IronText.Automata;
using IronText.Reflection;
using System;

namespace IronText.MetadataCompiler.Analysis
{
    public static class ProductionExtensions
    {
        public static BuildtimeProduction ToBuildtime(this Production production) =>
            new BuildtimeProduction(
                production.Index,
                production.Outcome.Index,
                production.InputTokens,
                MakeNode(production));

        static BuildtimeProductionNode MakeTree(IProductionComponent component) =>
            MakeNode((dynamic)component);

        static BuildtimeProductionNode MakeNode(Production production) =>
            new BuildtimeProductionNode(
                production.Outcome.Index,
                Array.ConvertAll(production.ChildComponents, MakeTree));

        static BuildtimeProductionNode MakeNode(Symbol symbol) =>
            new BuildtimeProductionNode(symbol.Index);

        static BuildtimeProductionNode MakeNode(IProductionComponent other)
        {
            throw new InvalidOperationException($"Unknown component type '{other.GetType().FullName}'");
        }
    }
}
