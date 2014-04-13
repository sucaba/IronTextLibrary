using IronText.Framework;

namespace IronText.Lib.Stem
{
    [Vocabulary]
    public class StemScanner
    {
        public const string LParen = "(";
        public const string RParen = ")";

        [Match("'//' ~[\r\n]*")]
        public void LineComment() { }

        [Match("'\r\n'")]
        public void NewLine() { }

        [Match("blank+")]
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
            quot")]
        public QStr Str(string text)
        { 
            return QStr.Parse(text); 
        }
    }
}
