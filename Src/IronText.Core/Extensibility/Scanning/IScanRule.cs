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
}
