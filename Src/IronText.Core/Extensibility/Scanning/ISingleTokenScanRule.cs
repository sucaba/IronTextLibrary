using System;
using System.Collections.Generic;
using System.Reflection;

namespace IronText.Extensibility
{
    public interface ISingleTokenScanRule : IScanRule
    {
        string LiteralText { get; }

        Type TokenType { get; }

        TokenRef AnyTokenRef { get; }
    }
}
