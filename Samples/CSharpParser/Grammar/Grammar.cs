using IronText.Framework;

namespace CSharpParser
{
    [Language]
    [StaticContext(typeof(CsCollections))]
    [StaticContext(typeof(CsPreprocessor))]
    [DescribeParserStateMachine("CSharp.info")]
    public partial interface ICsGrammar
    {
        [SubContext]
        CsScanner Scanner { get; }

        [ParseResult]
        CsCompilationUnit Result { get; set; }
    }
}
