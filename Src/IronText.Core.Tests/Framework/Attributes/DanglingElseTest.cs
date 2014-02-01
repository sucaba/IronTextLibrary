using IronText.Framework;
using IronText.Reflection;
using IronText.Runtime;
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
        [Precedence("then", 0, Associativity.None)]
        [Precedence("else", 1, Associativity.None)]
        public class DanglingElseLang
        {
            public Stmt Result { get; [Produce] set; }

            [Produce("if", null, "then", null, "else", null)]
            public Stmt IfThenElse(Expr cond, Stmt t, Stmt e) { return null; }

            [Produce("if", null, "then", null)]
            public Stmt IfThen(Expr cond, Stmt then) { return null; }

            [Produce("mystmt")]
            public Stmt MyStmt() { return null; }

            [Produce("myexpr")]
            public Expr MyExpr() { return null; }

            [Match("blank+")]
            public void Space() { }
        }
    }
}
