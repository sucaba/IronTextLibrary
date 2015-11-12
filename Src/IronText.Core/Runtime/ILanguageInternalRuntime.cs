
namespace IronText.Runtime
{
    internal interface ILanguageInternalRuntime
    {
        object         GetSourceGrammar();

        RuntimeGrammar RuntimeGrammar     { get; }
    }
}
