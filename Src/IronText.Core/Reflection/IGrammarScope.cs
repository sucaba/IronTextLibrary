
namespace IronText.Reflection
{
    public interface IGrammarScope
    {
        bool                 AreNonTermsBottomUpByDefault { get; }

        Symbol               Start       { get; }

        Production           AugmentedProduction { get; }

        SymbolCollection     Symbols     { get; }

        ProductionCollection Productions { get; }
        
        MatcherCollection    Matchers    { get; }

        SymbolPropertyCollection      SymbolProperties { get; }

        InheritedPropertyCollection   InheritedProperties { get;  }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// when grammar is still being edited.
        /// </exception>
        void RequireImmutable();
    }
}
