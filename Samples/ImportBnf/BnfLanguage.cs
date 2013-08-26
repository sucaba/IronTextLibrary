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

        [Parse]
        WantDocument BeginDocument();
    }

    [Demand]
    public interface WantDocument
    {
        [Parse(null, ":")]
        WantRule BeginRule(string tokenName);

        [Parse]
        void EndDocument();
    }

    [Demand]
    public interface WantRule
    {
        [Parse]
        WantBranch Branch();
    }
    
    [Demand]
    public interface WantRuleAlternative
    {
        [Parse("|")]
        WantBranch Branch();

        [Parse(";")]
        WantDocument EndRule();
    }

    [Demand]
    public interface WantBranch
    {
        [Parse]
        WantBranch Token(string tokenName);

        [Parse]
        WantBranch Literal(QStr literal);

        [Parse]
        WantRuleAlternative EndBranch();
    }
}
