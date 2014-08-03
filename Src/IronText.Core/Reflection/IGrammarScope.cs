
namespace IronText.Reflection
{
    public interface IGrammarScope
    {
        Symbol               Start       { get; }

        Production           AugmentedProduction { get; }

        SymbolCollection     Symbols     { get; }

        ProductionCollection Productions { get; }
        
        MatcherCollection    Matchers    { get; }
    }
}
