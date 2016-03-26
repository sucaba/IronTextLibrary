namespace IronText.Reflection
{
    internal interface IProductionSemanticScope
    {
        Symbol Outcome { get; }

        Symbol[] Input { get; }
    }
}
