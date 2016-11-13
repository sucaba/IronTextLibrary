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
                production.InputTokens);

        static TreeNode MakeTree(IProductionComponent component) =>
            MakeNode((dynamic)component);

        static TreeNode MakeNode(Production production) =>
            new TreeNode(
                production.Original.Index,
                production.Outcome.Index,
                Array.ConvertAll(production.ChildComponents, MakeTree));

        static TreeNode MakeNode(Symbol symbol) =>
            new TreeNode(symbol.Index);

        static TreeNode MakeNode(IProductionComponent other)
        {
            throw new InvalidOperationException($"Unknown component type '{other.GetType().FullName}'");
        }
    }
}
