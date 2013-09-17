using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.RegularAst;

namespace IronText.Lib.ScannerExpressions
{
    [Language]
    [GrammarDocument("ScannerSyntax.gram")]
    [ScannerDocument("ScannerSyntax.scan")]
    [StaticContext(typeof(Builtins))]
#if DEBUG
    [DescribeParserStateMachine("ScannerSyntax.info")]
    [ScannerGraph("ScannerSyntax_Scanner.gv")]
    [ParserGraph("ScannerSyntax_Parser.gv")]
#endif
    public class ScannerSyntax
    {
        private static readonly UnicodeIntSetType IntSet = UnicodeIntSetType.Instance;

        [ParseResult]
        public Regexp Result { get; set; }

        [Parse]
        public Regexp Regexp(Branch item) 
        { 
            var result = new Regexp();
            result.Branches.Add(item.Node);
            return result;
        }

        [Parse(null, "|", null)]
        public Regexp Regexp(Regexp regexp, Branch item) 
        {
            regexp.Branches.Add(item.Node);
            return regexp;
        }

        [Parse]
        public Branch EmptyBranch() 
        {
            return new Branch { Node = AstNode.Empty };
        }

        [Parse]
        public Branch Branch(Piece[] pieces) 
        {
            return new Branch { Node = AstNode.Cat(pieces.Select(p => p.Node)) };
        }

        [Parse(null, "?")]
        public Piece Optional(Piece atom) 
        {
            return new Piece { Node = RepeatNode.Optional(atom.Node) }; 
        }

        [Parse(null, "*")]
        public Piece ZeroOrMore(Piece atom)
        {
            return new Piece { Node = RepeatNode.ZeroOrMore(atom.Node) }; 
        }

        [Parse(null, "+")]
        public Piece OneOrMore(Piece atom)
        {
            return new Piece { Node = RepeatNode.OneOrMore(atom.Node) }; 
        }

        [Parse("(", null, ")")]
        public Piece Piece(Regexp regexp) 
        {
            return new Piece { Node = regexp.Node };
        }

        [Parse("action", "(", null, ")")]
        public Piece Action(Integer action)
        {
            return new Piece { Node = ActionNode.Create(action.Value) };
        }

        [Parse]
        public Piece Literal(QStr str) 
        { 
            var nodes = str.Text.Select(ch => CharSetNode.Create(IntSet.Of(ch)));
            return new Piece
            {
                Node = AstNode.Cat(nodes) 
            };
        }

        [Parse]
        public Piece Piece(IntSet charClass) 
        {
            if (charClass.IsEmpty)
            {
                return new Piece { Node = OrNode.Or() };
            }

            return new Piece { Node = CharSetNode.Create(charClass) };
        }

        [Parse("~")]
        public IntSet Complement(IntSet charClass) 
        { 
            return charClass.Complement();
        }

        [Parse("~", "(", null, ")")]
        public IntSet ComplementComposite(CompositeIntSet composite) 
        { 
            return composite.Inner.Complement();
        }

        [Parse]
        public CompositeIntSet CompositeIntSet(IntSet inner)
        {
            return new CompositeIntSet { Inner = inner };
        }

        [Parse(null, "|", null)]
        public CompositeIntSet CompositeIntSet(CompositeIntSet composite, IntSet inner)
        {
            return new CompositeIntSet { Inner = composite.Inner.Union(inner) };
        }

        [Parse(null, "..", null)]
        public IntSet Range(Chr from, Chr to)
        {
            return IntSet.Range(from.Char, to.Char);
        }

        [Parse]
        public IntSet SingleChar(Chr ch)
        {
            return IntSet.Of(ch.Char);
        }

        [Parse]
        public IntSet CharClass(CharEnumeration item)
        {
            return IntSet.Of(item.Characters);
        }

        [Scan(@"['] (~['\\] | [\\] .) [']",
              @"['] (?: [^'\\] | [\\] .) [']")]
        public Chr Char(char[] buffer, int start, int length)
        {
            return Chr.Parse(buffer, start, length);
        }

        [Scan(@"['] ~['\\]* ( [\\] .  ~['\\]* )* [']",
              @"['] (?: [^'\\]* (?: \\ . [^'\\]*)*) [']")]
        public QStr SingleQuotedString(char[] buffer, int start, int length)
        {
            return QStr.Parse(buffer, start, length); 
        }

        [Scan(@"'[' ~[\]\\]* ( [\\] . ~[\]\\]* )* ']'",
              @"\[ (?: [^\]\\]* (?: \\ . [^\]\\]*)*) \]")]
        public CharEnumeration CharClass(char[] buffer, int start, int length) 
        {
            return CharEnumeration.Parse(buffer, start, length);
        }

        [Scan(
            @"'u' hex hex hex hex",
            @"u [0-9a-fA-F]{4}")]
        public QStr UnicodeCharByCode(string text) 
        {
            int ch = (Hex(text[1]) << 12)
                   + (Hex(text[2]) << 8)
                   + (Hex(text[3]) << 4)
                   + Hex(text[4])
                   ;
            return new QStr(new string((char)ch, 1));
        }

        [Scan(
            @"'U' hex hex hex hex  hex hex hex hex",
            @"U [0-9a-fA-F]{8}")]
        public QStr UnicodeSurrogateByCode(string text) 
        {
            int value = (Hex(text[1]) << 28)
                      + (Hex(text[2]) << 24)
                      + (Hex(text[3]) << 20)
                      + (Hex(text[4]) << 16)
                      + (Hex(text[5]) << 12)
                      + (Hex(text[6]) << 8)
                      + (Hex(text[7]) << 4)
                      + Hex(text[8])
                      ;
            return new QStr(char.ConvertFromUtf32(value));
        }

        [Parse("alnum")]  public IntSet Alphanumeric() { return IntSet.AsciiAlnum; }

        [Parse("alpha")]  public IntSet Alphabetic() { return IntSet.AsciiAlpha; }

        [Parse("blank")]  public IntSet Blank() { return IntSet.AsciiBlank; }

        [Parse("digit")]  public IntSet Numeric() { return IntSet.AsciiDigit; }

        [Parse("esc")]    public IntSet BackSlash() { return IntSet.Of('\\'); }

        [Parse("hex")]    public IntSet HexDigit() { return IntSet.AsciiXDigit; }

        [Parse("print")]  public IntSet Print() { return IntSet.AsciiPrint; }

        [Parse("quot")]   public IntSet Quot() { return IntSet.Of('"'); }

        [Parse("zero")]   public IntSet Zero() { return IntSet.Of('\0'); }

        [Parse(".")]      public IntSet Any() { return IntSet.All; }

        [Scan(
            "digit+",
            @"[0-9]+")]
        public Integer Integer(string text) { return new Integer(int.Parse(text)); }

        [Scan(
            @"'\r'? '\n'",
            @"\r? \n")]
        public void NewLine() { }

        [Scan(
            @"blank+",
            @"[ \t]+")]
        public static void WhiteSpace() { }

        private static int Hex(char ch)
        {
            switch (ch)
            {
                case '0': return 0x0;
                case '1': return 0x1;
                case '2': return 0x2;
                case '3': return 0x3;
                case '4': return 0x4;
                case '5': return 0x5;
                case '6': return 0x6;
                case '7': return 0x7;
                case '8': return 0x8;
                case '9': return 0x9;
                case 'A':
                case 'a': return 0xa;
                case 'B':
                case 'b': return 0xb;
                case 'C':
                case 'c': return 0xc;
                case 'D':
                case 'd': return 0xd;
                case 'E':
                case 'e': return 0xe;
                case 'F':
                case 'f': return 0xf;
                default: return -1;
            }
        }
    }
    
    public class Regexp 
    {
        public readonly List<AstNode> Branches = new List<AstNode>();

        public AstNode Node
        {
            get { return AstNode.Or(Branches);  }
        }
    }

    public sealed class Branch { public AstNode Node; }

    public sealed class Piece  { public AstNode Node; }

    public sealed class CompositeIntSet { public IntSet Inner; }

    public sealed class CharEnumeration 
    { 
        public static CharEnumeration Parse(string text)
        {
            char[] chars = text.ToCharArray();
            return new CharEnumeration(QStr.Unescape(chars, 1, chars.Length - 2));
        }

        public static CharEnumeration Parse(char[] buffer, int start, int length)
        {
            return new CharEnumeration(QStr.Unescape(buffer, start + 1, length - 2));
        }

        public CharEnumeration(string characters)
        {
            this.Characters = characters;
        }

        public string Characters { get; private set; }
    }

    public sealed class Integer 
    { 
        public static Integer Parse(string text)
        {
            return new Integer(int.Parse(text));
        }

        public Integer(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
    }

    public sealed class Range { public char From; public char To; }
}
