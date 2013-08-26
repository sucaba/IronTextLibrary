using IronText.Framework;
using NUnit.Framework;

namespace IronText.Core.Tests.Framework.Attributes
{
    [TestFixture]
    public class DanglingElseTest
    {
        [Test]
        public void Test()
        {
            Eval("if myexpr then mystmt");
            Eval("if myexpr then mystmt else mystmt");
            Eval("if myexpr then if myexpr then mystmt else mystmt");
            Eval("if myexpr then if myexpr then mystmt else mystmt else mystmt");
        }

        public Stmt Eval(string input)
        {
            var context = new DanglingElseLang();
            Language.Parse(context, input);
            return context.Result;
        }

        public interface Stmt { }
        public interface Expr { }

        [Language]
        [DescribeParserStateMachine("DanglingElseLang.info")]
        [NonAssoc(0, "then")]
        [NonAssoc(1, "else")]
        public class DanglingElseLang
        {
            public Stmt Result { get; [Parse] set; }

            [Parse("if", null, "then", null, "else", null)]
            public Stmt IfThenElse(Expr cond, Stmt t, Stmt e) { return null; }

            [Parse("if", null, "then", null)]
            public Stmt IfThen(Expr cond, Stmt then) { return null; }

            [Parse("mystmt")]
            public Stmt MyStmt() { return null; }

            [Parse("myexpr")]
            public Expr MyExpr() { return null; }

            [Scan("blank+")]
            public void Space() { }
        }
    }
}
