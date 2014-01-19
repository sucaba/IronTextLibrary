using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface ICilScanRule
    {
        // Binding
        MethodInfo DefiningMember { get; }

        // Scan Produciton
        Disambiguation Disambiguation { get; }

        // Scan Produciton
        string Pattern { get; }

        CilScanActionBuilder ActionBuilder { get; }

        // Scan Produciton: Mode object
        Type NextModeType { get; }

        // Scan Produciton: MainSymbol
        CilSymbolRef MainTokenRef { get; }

        // Main produced token
        IEnumerable<CilSymbolRef[]> GetTokenRefGroups();

        int Index { get; set; }
    }
}
