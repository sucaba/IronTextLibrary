using System;
using System.Linq;
using IronText.Extensibility;
using IronText.Reflection;
using IronText.Reflection.Managed;
using NUnit.Framework;

namespace IronText.Tests.Extensions
{
    [TestFixture]
    public class SymbolResolverTest
    {
        CilSymbolRefResolver target;

        [SetUp]
        public void SetUp()
        {
            target = new CilSymbolRefResolver();
        }

        [Test]
        public void Test()
        {
            var tokens = new []
            {
                CilSymbolRef.Literal("foo"),
                CilSymbolRef.Literal("bar"),
                CilSymbolRef.Literal("other"),
                CilSymbolRef.Typed(typeof(string)),
                CilSymbolRef.Create(typeof(string), "foo"),   // links foo and string
                CilSymbolRef.Create(typeof(string), "bar"),   // links bar and string
                CilSymbolRef.Create(typeof(string), "other"), // links other and string
            };

            foreach (var token in tokens)
            {
                target.Link(token);
            }

            Assert.AreEqual(1, target.Definitions.Count());

            var SYM1 = new Symbol("123");
            target.SetId(tokens[0], SYM1);
            for (int i = 1; i != tokens.Length; ++i)
            {
                var context = "#" + i;
                Assert.AreSame(SYM1, target.GetSymbol(tokens[i]), context);
            }
        }

        [Test]
        public void TestLinkTwoTypes()
        {
            target.Link(CilSymbolRef.Create(typeof(string), "foo"));
            Assert.Throws<InvalidOperationException>(
                () => target.Link(CilSymbolRef.Create(typeof(int), "foo")));
        }
    }
}
