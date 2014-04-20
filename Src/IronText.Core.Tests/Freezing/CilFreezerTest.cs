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
                Assert.IsTrue(interp.Parse("1"));
                Assert.AreEqual("1", interp.Context.Outcome);
            }

            using (var freezer = new CilFreezer<MyCalc>())
            {
                Pipe<MyCalc> code = freezer.Compile("1+1");

                var context = new MyCalc();
                Assert.AreSame(context, code(context));
                Assert.AreEqual("11", context.Outcome);
            }
        }

        [Language]
        [GrammarDocument("FreezerTest.gram")]
        [Precedence("+", 1, Associativity.Left)]
        public class MyCalc
        {
            [Outcome]
            public object Outcome { get; set; }

            [Produce(null, "+", null)]
            public object Combine(object x, object y) { return x.ToString() + y.ToString(); }

            [Literal("1")]
            public object One(string text) { return text; }
        }
    }
}
