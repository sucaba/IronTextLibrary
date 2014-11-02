namespace IronText.Reflection
{
    internal interface IInjectedActionParameterTextMatcher
    {
        bool Match(InjectedActionParameter p, string text);
    }
}
