using IronText.Reflection;

namespace IronText.Analysis
{
    interface IGrammarConverter
    {
        T Convert<T>(T source) where T : SymbolBase;

        Production Convert(Production source);
    }
}
