using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Freezing.Managed;
using IronText.Runtime;
using NUnit.Framework;
using IronText.Reflection;

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
                Assert.IsTrue(interp.Parse("1|2^3|4"));
                Assert.AreEqual("(1|(2^3)|4)", interp.Context.Outcome);
            }

            using (var freezer = new CilFreezer<MyCalc>())
            {
                Pipe<MyCalc> code = freezer.Compile("1|2^3|4");

                var context = new MyCalc();
                Assert.AreSame(context, code(context));
                Assert.AreEqual("(1|(2^3)|4)", context.Outcome);
            }
        }

        [Language]
        [GrammarDocument("FreezerTest.gram")]
        [Precedence("|", 1, Associativity.Left)]
        [Precedence("^", 2, Associativity.Left)]
        public class MyCalc
        {
            [Outcome]
            public object Outcome { get; set; }

            [Produce(null, "^", null)]
            public object Pow(object x, object y) { return string.Format("({0}^{1})", x, y); }

            [Produce(null, "|", null, "|", null)]
            public object Tuple(object x, object y, object z) { return string.Format("({0}|{1}|{2})", x, y, z); }

            [Match("digit")]
            public object Digit(string text) { return text; }
        }
    }
}
