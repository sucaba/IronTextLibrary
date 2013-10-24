using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface IScanRule
    {
        MemberInfo DefiningMember { get; }

        Disambiguation Disambiguation { get; }

        string Pattern { get; }

        ScanActionBuilder ActionBuilder { get; }

        Type NextModeType { get; }

        TokenRef MainTokenRef { get; }

        IEnumerable<TokenRef[]> GetTokenRefGroups();

        // TODO: Make internal
        int Priority { get; set; }
        bool IsSortable { get; }
    }
}
