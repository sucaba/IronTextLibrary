
namespace IronText.Reflection
{
    public interface IGrammarScope
    {
        Symbol               Start       { get; }

        SymbolCollection     Symbols     { get; }

        ProductionCollection Productions { get; }
        
        MatcherCollection    Matchers    { get; }
    }
}
