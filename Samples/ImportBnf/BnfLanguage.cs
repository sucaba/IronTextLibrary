using IronText.Framework;
using IronText.Lib.Ctem;

namespace Samples
{
    [Language]
    // [DescribeParserStateMachine("BnfLanguage.info")]
    // [ScannerDocument("BnfLanguage.scan")]
    // [ScannerGraph("BnfLanguage.gv")]
    public interface BnfLanguage
    {
        [SubContext]
        CtemScanner Scanner { get; }

        [Produce]
        WantDocument BeginDocument();
    }

    [Demand]
    public interface WantDocument
    {
        [Produce(null, ":")]
        WantRule BeginRule(string tokenName);

        [Produce]
        void EndDocument();
    }

    [Demand]
    public interface WantRule
    {
        [Produce]
        WantBranch Branch();
    }
    
    [Demand]
    public interface WantRuleAlternative
    {
        [Produce("|")]
        WantBranch Branch();

        [Produce(";")]
        WantDocument EndRule();
    }

    [Demand]
    public interface WantBranch
    {
        [Produce]
        WantBranch Token(string tokenName);

        [Produce]
        WantBranch Literal(QStr literal);

        [Produce]
        WantRuleAlternative EndBranch();
    }
}
