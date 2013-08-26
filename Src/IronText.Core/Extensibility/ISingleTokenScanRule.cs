using System;
using System.Collections.Generic;

namespace IronText.Extensibility
{
    public interface IScanRule
    {
        string Pattern { get; }

        ScanActionBuilder ActionBuilder { set; }

        Type NextModeType { get; }

        IEnumerable<TokenRef[]> GetTokenRefGroups();
    }

    public interface ISkipScanRule : IScanRule
    {
    }

    public interface ISingleTokenScanRule : IScanRule
    {
        string LiteralText { get; }

        Type TokenType { get; }

        TokenRef AnyTokenRef { get; }
    }
}
