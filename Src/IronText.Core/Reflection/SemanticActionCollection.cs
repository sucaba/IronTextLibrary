using System;

namespace IronText.Reflection
{
    [Serializable]
    internal class SemanticActionCollection : GrammarEntityCollection<SemanticAction, IGrammarScope>
    {
        public SemanticActionCollection(IGrammarScope scope)
            : base(scope)
        {
        }

        internal SemanticAction Add(string productionText, string fromExpr, string toExpr)
        {
            var resolver = DR.Resolve<IProductionResolver>();
            var production = resolver.Find(productionText);
            var from       = ParseProp(fromExpr);
            var to         = ParseProp(toExpr);
            var item       = SemanticAction.MakeCopyOutToOut(production, from, to);
            return Add(item);
        }

        private ISymbolProperty ParseProp(string dotExpr)
        {
            var parts = DotExpression.Parse(dotExpr);
            switch (parts[1])
            {
                case "!": return new InheritedProperty(Scope.Symbols.ByName(parts[0]), parts[1]);
                default:  return new SymbolProperty(Scope.Symbols.ByName(parts[0]), parts[1]);
            }
        }
    }
}
