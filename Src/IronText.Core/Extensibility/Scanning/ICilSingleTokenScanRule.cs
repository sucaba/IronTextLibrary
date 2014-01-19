using System;
using System.Collections.Generic;
using System.Reflection;

namespace IronText.Extensibility
{
    public interface ICilSingleTokenScanRule : ICilScanRule
    {
        string LiteralText { get; }

        Type TokenType { get; }

        CilSymbolRef AnyTokenRef { get; }
    }
}
