namespace IronText.Reflection
{
    internal interface IProductionSemanticScope
    {
        Symbol Outcome { get; }

        Symbol[] Input { get; }

        ISymbolProperty ResolveProperty(Symbol symbol, string name, bool isInherited);
    }
}
