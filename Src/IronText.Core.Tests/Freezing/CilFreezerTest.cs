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
                Assert.IsTrue(interp.Parse("1"));
                Assert.IsNull(interp.Context.Outcome);
            }

            using (var freezer = new CilFreezer<MyCalc>())
            {
                Pipe<MyCalc> code = freezer.Compile("1");

                var context = new MyCalc();
                Assert.IsNull(code(context));
            }
        }

        [Language]
        [GrammarDocument("FreezerTest.gram")]
        [Precedence("-", 1)]
        [Precedence("+", 1)]
        [Precedence("*", 2)]
        [Precedence("/", 2)]
        public class MyCalc
        {
            [Outcome]
            public object Outcome { get; set; }

            [Literal("1")]
            public object One() { return null; }
        }
    }
}
