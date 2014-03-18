using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Freezing.Managed;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Freezing
{
    [TestFixture]
    public class CilFreezerTest
    {
        [Test]
        public void Test()
        {
            using (var interp = new Interpreter<MyCalc>())
            {
                Assert.IsTrue(interp.Parse("1 + 2 * 3"));
                Assert.AreEqual(7, interp.Context.Outcome);
            }

            using (var freezer = new CilFreezer<MyCalc>())
            {
                Pipe<MyCalc> code = freezer.Compile("1 + 2 * 3");

                var context = new MyCalc();
                Assert.AreEqual(7, code(context).Outcome);
            }
        }

        [Language]
        [Precedence("-", 1)]
        [Precedence("+", 1)]
        [Precedence("*", 2)]
        [Precedence("/", 2)]
        public class MyCalc
        {
            [Outcome]
            public double Outcome { get; set; }

            [Produce(null, "*", null)]
            public double Multiply(double x, double y)  { return x * y; }

            [Produce(null, "/", null)]
            public double Divide(double x, double y)    { return x / y; }

            [Produce(null, "+", null)]
            public double Add(double x, double y)       { return x + y; }

            [Produce(null, "-", null)]
            public double Substract(double x, double y) { return x - y; }

            [Match("digit+ ('.' digit*)?")]
            public double Number(string text) { return double.Parse(text); }

            [Match("blank+")]
            public void Blank() { }
        }

        public class Expr { }
    }
}
