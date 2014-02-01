using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Lib.RegularAst;
using IronText.Lib.Stem;

namespace IronText.Lib.Sre
{
    public class SreExpr { public AstNode Node; }

    public class OpenSreExpr { public AstNode Node; }

    public class CSetSreExpr { public CharSetNode Node; }

    [Language]
    [StaticContext(typeof(Builtins))]
    public class SreSyntax : StemScanner
    {
        public static string Escape(string value)
        {
            return "\"" + string.Concat(value.Select(EscapeChar)) + "\"";
        }

        private static string EscapeChar(char value)
        {
            switch (value)
            {
                case '\n': return @"\n";
                case '\r': return @"\r";
                case '"': return @"\""";
                case '\\': return @"\\";
            }

            return new string(value, 1);
        }

        private UnicodeIntSetType IntSet = UnicodeIntSetType.Instance;

        public OpenSreExpr Result { get; [Produce] set; }

        [Produce]
        public OpenSreExpr OpenSeq(SreExpr[] items)
        {
            return new OpenSreExpr { Node = AstNode.Cat(items.Select(item => item.Node).ToArray()) };
        }

        [Actor("action")]
        public SreExpr Action(Num num)
        {
            return new SreExpr { Node = ActionNode.Create(int.Parse(num.Text)) };
        }

        [Produce]
        public SreExpr LiteralMatch(QStr str)
        {
            return new SreExpr { Node = new CatNode(str.Text.Select(ch => CharSetNode.Create(ch))) };
        }

        [Produce]
        public SreExpr CSet(CSetSreExpr cset)
        {
            return new SreExpr { Node = cset.Node };
        }

        [Actor("*")]
        public SreExpr ZeroOrMore(OpenSreExpr items)
        {
            return new SreExpr { Node = RepeatNode.ZeroOrMore(items.Node) };
        }

        [Actor("+")]
        public SreExpr OneOrMore(OpenSreExpr items)
        {
            return new SreExpr { Node = RepeatNode.OneOrMore(items.Node) };
        }

        [Actor("?")]
        public SreExpr ZeroOrOne(OpenSreExpr inner)
        {
            return new SreExpr { Node = RepeatNode.Optional(inner.Node) };
        }

        [Actor("=")]
        public SreExpr NMatches(Num n, OpenSreExpr inner)
        {
            int count = int.Parse(n.Text);
            return new SreExpr { Node = new RepeatNode(inner.Node, count, count) };
        }

        [Actor(">=")]
        public SreExpr NOrMoreMatches(Num n, OpenSreExpr inner)
        {
            int count = int.Parse(n.Text);
            return new SreExpr { Node = new RepeatNode(inner.Node, count, int.MaxValue) };
        }

        [Actor("**")]
        public SreExpr NToMMatches(Num n, Num m, OpenSreExpr inner)
        {
            int from = int.Parse(n.Text);
            int to = int.Parse(m.Text);
            return new SreExpr { Node = new RepeatNode(inner.Node, from, to) };
        }

        [Actor("|")]
        public SreExpr Or1(SreExpr[] inner)
        {
            return new SreExpr { Node = new OrNode(inner.Select(token => token.Node)) };
        }

        [Actor("or")]
        public SreExpr Or2(SreExpr[] inner) { return Or1(inner); }

        [Actor("look-back")]
        public SreExpr Lookback(CSetSreExpr cset)
        {
            return new SreExpr { Node = new LookbackNode(cset.Node.Characters) };
        }

        [Actor("look-ahead")]
        public SreExpr Lookahead(CSetSreExpr cset)
        {
            return new SreExpr { Node = new LookaheadNode(cset.Node.Characters) };
        }

        [Actor(":")]
        public SreExpr Seq1(OpenSreExpr inner) { return new SreExpr { Node = inner.Node }; }

        [Actor("seq")]
        public SreExpr Seq2(OpenSreExpr inner) { return Seq1(inner); }

        // [Actor("submatch")]
        // public SreExpr NumberedSubmatch(SreExpr[] inner) { throw new NotImplementedException(); }

        // [Actor("dsm")]
        // public SreExpr DeletedSubMatches(Num pre, Num post, SreExpr[] inner) { throw new NotImplementedException(); }

        // [Actor("case")]
        // public SreExpr CaseFoldedMatch(SreExpr[] inner) { throw new NotImplementedException(); }

        // [Actor("w/case")]
        // public SreExpr CaseSensitiveContext(SreExpr[] inner) { throw new NotImplementedException(); }

        [Actor("w/nocase")]
        public SreExpr CaseUnSensitiveContext(SreExpr[] inner) { return new SreExpr(); }

        // [Actor("word")]
        // public SreExpr Word(SreExpr[] inner) { throw new NotImplementedException(); }

        // Following assertions require lookahead functionality 
        /// <summary>
        /// Null-length assertion
        /// before: input start, space or delimiter
        /// after: 
        /// </summary>
        /// <returns></returns>
        [Produce("bow")] public SreExpr Bow() { return new SreExpr { Node = new LookaheadNode(IntSet.AsciiAlnum) }; }
        // [Pattern("eow")] public SreExpr Eow() { throw new NotImplementedException(); }
        //[Pattern("bos")] public SreExpr Bos() { throw new NotImplementedException(); }
        //[Pattern("eos")] public SreExpr Eos() { throw new NotImplementedException(); }

        [Produce]
        public CSetSreExpr SingletonCharSet(Chr ch) { return CSet(IntSet.Of(ch.Text[0])); }

        [Produce("(", null, ")")]
        public CSetSreExpr CharSet(QStr str) { return CSet(IntSet.Of(str.Text)); }

        [Actor("~")]
        public CSetSreExpr ComplementOfUnion(CSetSreExpr[] inner) 
        {
            return new CSetSreExpr { Node = CharSetNode.Union(inner.Select(expr => expr.Node)).Complement() };
        }

        [Actor("-")]
        public CSetSreExpr Difference(CSetSreExpr x, CSetSreExpr[] y)
        { 
            IntSet cset = x.Node.Characters;
            foreach (var excluded in y)
            {
                cset = cset.Except(excluded.Node.Characters);
            }

            return CSet(cset);
        }

        [Actor("&")]
        public CSetSreExpr Intersection(CSetSreExpr[] inner)
        {
            IntSet cset = IntSet.All;
            foreach (var included in inner)
            {
                cset = cset.Intersect(included.Node.Characters);
            }

            return CSet(cset);
        }

        [Actor("/")]
        public CSetSreExpr CharRanges(IntSet[] ranges)
        {
            return CSet(ranges.Aggregate(IntSet.Empty, (x, y) => x.Union(y)));
        }

        [Produce]
        public IntSet Range(Chr ch) { return IntSet.Of(ch.Text[0]); }

        [Produce]
        public IntSet Range(QStr charPairs)
        {
            var intervals = new List<IntInterval>();
            string text = charPairs.Text;
            int len = text.Length;
            if (len % 2 != 0)
            {
                throw new Exception("Invalid range pairs.");
            }

            for (int i = 0; i < len; i+=2)
            {
                intervals.Add(new IntInterval(text[i], text[i + 1]));
            }

            var cset = IntSet.Ranges(intervals.ToArray());
            return cset;
        }

        [Produce("any")]          public CSetSreExpr Any() { return CSet(IntSet.All); }

        [Produce("quot")]         public CSetSreExpr Quot() { return CSet(IntSet.Of('"')); }
        [Produce("esc")]          public CSetSreExpr BackSlash() { return CSet(IntSet.Of('\\')); }
        [Produce("zero")]         public CSetSreExpr Zero() { return CSet(IntSet.Of('\0')); }

        [Produce("ascii")]        public CSetSreExpr Ascii() { return CSet(IntSet.Ascii); }

        [Produce("nonl")]         public CSetSreExpr Nonl() { return CSet(IntSet.Of('\n').Complement()); }

        [Produce("lower-case")]   public CSetSreExpr LowerCase() { return CSet(IntSet.AsciiLower); }
        [Produce("lower")]        public CSetSreExpr Lower() { return LowerCase(); }

        [Produce("upper-case")]   public CSetSreExpr UpperCase() { return CSet(IntSet.AsciiUpper); }
        [Produce("upper")]        public CSetSreExpr Upper() { return UpperCase(); }

        [Produce("alpha")]        public CSetSreExpr Alpha() { return Alphabetic(); }
        [Produce("alphabetic")]   public CSetSreExpr Alphabetic() { return CSet(IntSet.AsciiAlpha); }

        [Produce("numeric")]      public CSetSreExpr Numeric() { return CSet(IntSet.AsciiDigit); }
        [Produce("digit")]        public CSetSreExpr Digit() { return Numeric(); }
        [Produce("num")]          public CSetSreExpr Num() { return Numeric(); }

        [Produce("alphanumeric")] public CSetSreExpr Alphanumeric() { return CSet(IntSet.AsciiAlnum); }
        [Produce("alnum")]        public CSetSreExpr Alnum() { return Alphanumeric(); }
        [Produce("alphanum")]     public CSetSreExpr Alphanum() { return Alphanumeric(); }

        [Produce("punctuation")]  public CSetSreExpr Punctuation() { return CSet(IntSet.AsciiPunct); }
        [Produce("punct")]        public CSetSreExpr Punct() { return Punctuation(); }

        [Produce("graphic")]      public CSetSreExpr Graphic() { return CSet(IntSet.AsciiGraph); }
        [Produce("graph")]        public CSetSreExpr Graph() { return Graphic(); }

        [Produce("whitespace")]   public CSetSreExpr Whitespace() { return CSet(IntSet.AsciiSpace); }
        [Produce("space")]        public CSetSreExpr Space() { return Whitespace(); }
        [Produce("white")]        public CSetSreExpr White() { return Whitespace(); }

        [Produce("blank")]        public CSetSreExpr Blank() { return CSet(IntSet.AsciiBlank); }

        [Produce("printing")]     public CSetSreExpr Printing() { return CSet(IntSet.AsciiPrint); }
        [Produce("print")]        public CSetSreExpr Print() { return CSet(IntSet.AsciiPrint); }

        [Produce("control")]      public CSetSreExpr Control() { return CSet(IntSet.AsciiCntrl); }
        [Produce("cntrl")]        public CSetSreExpr Cntrl() { return Control(); }

        [Produce("hex-digit")]    public CSetSreExpr HexDigit() { return CSet(IntSet.AsciiXDigit); }
        [Produce("xdigit")]       public CSetSreExpr Xdigit() { return HexDigit(); } 
        [Produce("hex")]          public CSetSreExpr Hex() { return HexDigit(); }

        private CSetSreExpr CSet(IntSet cset)
        {
            return new CSetSreExpr { Node = CharSetNode.Create(cset) };
        }

        [Match("'#' esc 'return'")]
        public Chr CR() { return new Chr('\r'); }

        [Match("'#' esc 'linefeed'")]
        public Chr NL() { return new Chr('\n'); }

        [Match("'#' esc print")]
        public Chr Char(char[] buffer, int start, int length) 
        { 
            return new Chr(buffer[start + 2]); 
        }
    }
}
