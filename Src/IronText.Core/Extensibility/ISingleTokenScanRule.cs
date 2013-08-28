using System;
using System.Collections.Generic;
using System.Reflection;

namespace IronText.Extensibility
{
    public interface IScanRule
    {
        MemberInfo DefiningMember { get; }

        string Pattern { get; }

        ScanActionBuilder ActionBuilder { get; }

        Type NextModeType { get; }

        IEnumerable<TokenRef[]> GetTokenRefGroups();

        // TODO: Make internal
        int Priority { get; set; }
        bool IsSortable { get; }
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
