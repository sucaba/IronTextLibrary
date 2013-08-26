using IronText.Framework;

namespace IronText.Lib.Stem
{
    [Vocabulary]
    public class StemScanner
    {
        public const string LParen = "(";
        public const string RParen = ")";

        [Scan("'//' ~[\r\n]*")]
        public void LineComment() { }

        [Scan("'\r\n'")]
        public void NewLine() { }

        [Scan("blank+")]
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
            quot")]
        public QStr Str(char[] buffer, int start, int length)
        { 
            return QStr.Parse(buffer, start, length); 
        }
    }
}
