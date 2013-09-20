using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public class CsScanner
    {
        [Scan(@"'//' ~('\r' | '\n' | u0085 | u2028 | u2029)*")]
        public void LineComment() { }

        [Scan("'/*' (~'*'* | '*' ~'/')* '*/'")]
        public void MultiLineComment() { }

        [Scan(@"'\r'? '\n' | u0085 | u2028 | u2029")]
        public void NewLine() { }

        [Scan("(Zs | '\t' | u000B | u000C)+")]
        public void WhiteSpace() { }

        [Scan("digit+ ('.' digit+)?  | '.' digit+")]
        public CsNumber Number(string token) { return null; }

        [Scan(
            @"
            quot 
                ~(quot | esc)*
                (esc . ~(quot | esc)* )*
            quot
            ")]
        public CsString Str(char[] buffer, int start, int length)
        {
            return null;
        }

        [Scan(@"
                '@'?
                ('_' | Lu | Ll | Lt | Lm | Lo | Nl)
                (Pc | Lu | Ll | Lt | Lm | Lo | Nl | Nd | Mn | Mc | Cf)*
               ")]
        public CsIdentifier Identifier(string text) { return null; }

        [Literal("true")]
        public CsBoolean BooleanTrue(string text) { return null; }

        [Literal("false")]
        public CsBoolean BooleanFalse(string text) { return null; }

        [Scan("digit+ ([Uu] | [Ll]  | 'UL' | 'Ul' | 'uL' | 'ul' | 'LU' | 'Lu' | 'lU' | 'lu')?")]
        public CsInteger DecimalInteger(string text) { return null; }

        [Scan("'0x' hex+ ([Uu] | [Ll]  | 'UL' | 'Ul' | 'uL' | 'ul' | 'LU' | 'Lu' | 'lU' | 'lu')?")]
        public CsInteger HexInteger(string text) { return null; }

        [Scan("digit+ '.' digit+ ([eE] [+-]? digit+)? [FfDdMm]?")]
        [Scan("       '.' digit+ ([eE] [+-]? digit+)? [FfDdMm]?")]
        [Scan("digit+            ([eE] [+-]? digit+)  [FfDdMm]?")]
        [Scan("digit+                                 [FfDdMm]")]
        public CsReal Real(string text) { return null; }

        [Scan(@"['] 
                ( ~(u0027 | u005c | '\n')
                | '\' ['""\0abfnrtv]
                | '\' hex {1,4}
                | '\' 'u' hex {4}
                )
                [']")]
        public CsChar Char(string text) { return null; } 

        [Scan(@"
                quot
                ( ~(quot | u005c | '\n')
                | '\' ['""\0abfnrtv]
                | '\' hex {1,4}
                | '\' 'u' hex {4}
                )*
                quot
              ")]
        public CsString RegularString(string text) { return null; }

        [Scan("'@' quot (~quot | quot quot)* quot")]
        public CsString VerbatimString(string text) { return null; }

        [Literal("null")]
        public CsNull Null() { return null; }

        [Scan("','+")]
        public CsDimSeparators DimSeparators() { return null; }

    #region Typed keywords

        [Literal("extern")]
        public CsExtern Extern() { return null; }

        [Literal("partial")]
        public CsPartial Partial() { return null; }

        [Literal(";")]
        public CsSemicolon Semicolon() { return null; }

        [Literal("new")]
        public CsNew NewKeyword() { return null; }
    #endregion
    }
}
