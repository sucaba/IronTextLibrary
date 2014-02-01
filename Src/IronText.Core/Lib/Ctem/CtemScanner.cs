using IronText.Framework;

namespace IronText.Lib.Ctem
{
    [Vocabulary]
    public class CtemScanner
    {
        [Match(@"'//' ~('\r' | '\n' | u0085 | u2028 | u2029)*")]
        public void LineComment() { }

        [Match("'/*' (~'*'* | '*' ~'/')* '*/'")]
        public void MultiLineComment() { }

        [Match(@"'\r'? '\n' | u0085 | u2028 | u2029")]
        public void NewLine() { }

        [Match("(Zs | '\t' | u000B | u000C)+")]
        public void WhiteSpace() { }

        [Match("digit+ ('.' digit+)?  | '.' digit+")]
        public Num Number(string token) { return new Num(token); }

        [Match("(alpha | '_') (alnum | '_')*")]
        public string Identifier(string token) { return token; }

        [Match(
            @"
            quot 
                ~(quot | esc)*
                (esc . ~(quot | esc)* )*
            quot
            ")]
        public QStr Str(char[] buffer, int start, int length)
        {
            return QStr.Parse(buffer, start, length);
        }
    }
}
