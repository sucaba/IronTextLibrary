namespace IronText.Reflection
{
    internal interface ISymbolTextMatcher
    {
        bool Match(Symbol symbol, string text);
    }
}
