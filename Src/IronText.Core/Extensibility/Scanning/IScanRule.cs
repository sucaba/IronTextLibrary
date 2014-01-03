using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface IScanRule
    {
        // Binding
        MethodInfo DefiningMember { get; }

        // Scan Produciton
        Disambiguation Disambiguation { get; }

        // Scan Produciton
        string Pattern { get; }

        ScanActionBuilder ActionBuilder { get; }

        // Scan Produciton: Mode object
        Type NextModeType { get; }

        // Scan Produciton: MainSymbol
        TokenRef MainTokenRef { get; }

        // Main produced token
        IEnumerable<TokenRef[]> GetTokenRefGroups();

        int Index { get; set; }
    }
}
