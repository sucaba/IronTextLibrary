using IronText.Reflection;

namespace IronText.Analysis
{
    internal interface IProductionInliner
    {
        Grammar Inline();
    }
}
