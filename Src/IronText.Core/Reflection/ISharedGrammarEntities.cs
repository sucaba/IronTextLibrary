
namespace IronText.Reflection
{
    public interface ISharedGrammarEntities
    {
        Symbol                      Start               { get; }

        SymbolCollection            Symbols             { get; }

        ProductionCollection        Productions         { get; }
        
        MatcherCollection           Matchers            { get; }
    }
}
