using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    [Language]
    [ScannerDocument("CSharpTokenizer.scan")]
    [ScannerGraph("CSharpTokenizer.gv")]
    [UseToken(typeof(CsNumber))]
    [UseToken(typeof(CsString))]
    [UseToken(typeof(CsIdentifier))]
    [UseToken(typeof(CsBoolean))]
    [UseToken(typeof(CsInteger))]
    [UseToken(typeof(CsReal))]
    [UseToken(typeof(CsChar))]
    [UseToken(typeof(CsNull))]
    [UseToken(typeof(CsDimSeparators))]
    [UseToken(typeof(CsExtern))]
    [UseToken(typeof(CsPartial))]
    [UseToken(typeof(CsSemicolon))]
    [UseToken(typeof(CsNew))]
    public interface ICsTokenizer
    {
        [SubContext]
        CsScanner Scanner { get; }

        [Parse]
        void Done();
    }
}
