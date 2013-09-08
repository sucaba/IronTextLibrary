using IronText.Framework;
using IronText.Lib.Ctem;
using NUnit.Framework;

namespace IronText.Tests.Framework.Attributes
{
    [TestFixture]
    public class ParseAttributeTest
    {
        [Test]
        public void Test()
        {
            Assert.IsTrue(Language.Get(typeof(ParseAttributeTestLang)).IsDeterministic);

            Language.Parse(
                new ParseAttributeTestLang(),
                @"if true 
                    print ""foo"" 
                  else 
                    print ""bar""
                ");
            Language.Parse(
                new ParseAttributeTestLang(),
                @"if true
                    if true
                        print ""foo"" 
                    else 
                        print ""bar"" 
                  else 
                    if true
                        print ""foo"" 
                    else 
                        print ""bar"" 
                 ");
        }

        public interface Stmt { }
        public interface Expr<T> {}

        [Language]
        [DescribeParserStateMachine("ParseAttributeTestLang.info")]
        [ParserGraph("ParseAttributeTestLang.gv")]
        [ScannerDocument("ParseAttributeTestLang.scan")]
        [Precedence("else", 2, Associativity.None)]
        public class ParseAttributeTestLang : CtemScanner
        {
            public Stmt Result { get; [Parse] set; }

            [Parse("true")]
            public Expr<bool> TrueBoolExpr() { return null; }

            [Parse(Precedence = 1, KeywordMask= new [] { "if", null, null })]
            public Stmt IfStmt(Expr<bool> cond, Stmt @then) { return null; }

            [Parse("if", null, null, "else", null)]
            public Stmt IfStmt(Expr<bool> cond, Stmt @then, Stmt @else) { return null; }

            [Parse("print")]
            public Stmt PrintStmt(QStr message) { return null; }
        }
    }
}
