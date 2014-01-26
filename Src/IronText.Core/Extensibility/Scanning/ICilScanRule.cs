using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface ICilScanRule
    {
        CilSymbolRef              MainOutcome    { get; }

        List<CilSymbolRef>        AllOutcomes    { get; }

        Type                      NextModeType   { get; }

        string                    Pattern        { get; }

        MethodInfo                DefiningMethod { get; }

        Disambiguation            Disambiguation { get; }

        CilScanActionBuilder      Builder        { get; }
    }
}
