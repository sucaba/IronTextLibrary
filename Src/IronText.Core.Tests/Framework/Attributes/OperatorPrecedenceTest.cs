using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class OperatorPrecedenceTest
    {
        [Test]
        public void Test()
        {
            Assert.AreEqual(7, Eval("1 + 2 * 3"));
            Assert.AreEqual(9, Eval("1 + 2 ^ 3"));
            Assert.AreEqual(512, Eval("2 ^ 3 ^ 2"));
            Assert.AreEqual(1, Eval("8 / 4 / 2"));
            Assert.AreEqual(4, Eval("8 / 4 * 2"));
            Assert.AreEqual(7, Eval("3 + 2 2"));
            Assert.AreEqual(8, Eval("3 + 2 2 + 1"));
            Assert.AreEqual(8, Eval("2 2 2"));
        }

        private int Eval(string input)
        {
            var context = new OperatorPrecedenceLang();
            Language.Parse(context, input);
            return context.Result;
        }

        [Test]
        public void TestReflection()
        {
            var lang = Language.Get(typeof(OperatorPrecedenceLang));
            int PLUS = lang.Identify("+");
            int MULT = lang.Identify("*");
            var plusPrecedence = lang.GetGrammar().Symbols[PLUS].Precedence;
            var multPrecedence = lang.GetGrammar().Symbols[MULT].Precedence;
            Assert.IsNotNull(plusPrecedence);
            Assert.IsNotNull(multPrecedence);
            Assert.AreEqual(Associativity.Left, plusPrecedence.Assoc);
            Assert.AreEqual(Associativity.Left, multPrecedence.Assoc);
            Assert.AreEqual(0, plusPrecedence.Value);
            Assert.AreEqual(1, multPrecedence.Value);
        }

        [Language]
        [Precedence("+", 0)]
        [Precedence("*", 1)]
        [Precedence("/", 1)]
        [Precedence("^", 2, Associativity.Right)]
        [Precedence(typeof(Num), 1)]
        [DescribeParserStateMachine("OperatorPrecedenceLang.info")]
        public class OperatorPrecedenceLang
        {
            [Outcome]
            public int Result { get; set; }

            [Produce(null, "+", null)]
            public int Plus(int x, int y) { return x + y; }

            [Produce(null, "*", null)]
            [Produce(Associativity.Left, Precedence = 1)]
            public int Multiply(int x, int y) { return x * y; }

            [Produce(null, "/", null)]
            public int Divide(int x, int y) { return x / y; }

            [Produce(null, "^", null)]
            public int Pow(int x, int y) 
            {
                int result = 1;
                while (y-- != 0)
                {
                    result *= x;
                }

                return result; 
            }

            [Produce("(", null, ")")]
            public int Parenthes(int inner) { return inner; }

            [Produce]
            public int Integer(Num num) { return int.Parse(num.Text); }

            [Match("digit+")]
            public Num Num(string text) { return new Num(text); }

            [Match("blank+")]
            public void Space() { }
        }
    }
}
