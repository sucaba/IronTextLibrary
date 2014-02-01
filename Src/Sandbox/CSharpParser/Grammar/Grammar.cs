using IronText.Framework;

namespace CSharpParser
{
    [Language(LanguageFlags.AllowNonDeterministic)]
    [StaticContext(typeof(CsCollections))]
    [StaticContext(typeof(CsPreprocessor))]
    [DescribeParserStateMachine("CSharp.info")]
    [GrammarDocument("CSharp.gram")]
    [ScannerDocument("CSharp.scan")]
    public partial interface ICsGrammar
    {
        [SubContext]
        CsScanner Scanner { get; }

        [Outcome]
        CsCompilationUnit Result { get; set; }
    }
}
