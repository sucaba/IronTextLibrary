using IronText.Framework;

namespace IronText.Lib.Ctem
{
    [Vocabulary]
    public class CtemScanner
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
        public Num Number(string token) { return new Num(token); }

        [Scan("(alpha | '_') (alnum | '_')*")]
        public string Identifier(string token) { return token; }

        [Scan(
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
