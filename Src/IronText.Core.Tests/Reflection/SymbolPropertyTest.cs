using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Reflection
{
    [TestFixture]
    public class SymbolPropertyTest
    {
        private const string SymName = "mySymb";
        private const string Name    = "myProp";

        private Grammar grammar;

        [SetUp]
        public void SetUp()
        {
            this.grammar =  new Grammar();
        }

        [Test]
        public void AddByDotExpressionCreatesAttributeWithProperySymbolAndNameTest()
        {
            var attr = grammar.SymbolProperties.Add(SymName + "." + Name);
            Assert.AreEqual(Name, attr.Name);
            Assert.AreEqual(SymName, attr.Symbol.Name);
        }

        [Test]
        public void AddByDotExpressionWithSpacesCreatesAttributeWithProperySymbolAndNameTest()
        {
            const string Spaces = "\r\n \t";
            var attr = grammar.SymbolProperties.Add(Spaces + SymName + Spaces + "." + Spaces + Name + Spaces);
            Assert.AreEqual(Name, attr.Name);
            Assert.AreEqual(SymName, attr.Symbol.Name);
        }

        [Test]
        public void DefineByDotExpressionCreatesMissingSymbolTest()
        {
            var attr = grammar.SymbolProperties.Add(SymName + "." + Name);
            Assert.IsNotNull(grammar.Symbols.ByName(SymName));
        }

        [Test]
        public void AddByInvalidDotExpressionCausesExceptionTest()
        {
            Assert.Throws<ArgumentException>(()=> { grammar.SymbolProperties.Add(SymName +  Name); }, "Missing dot case");
            Assert.Throws<ArgumentException>(()=> { grammar.SymbolProperties.Add(SymName + ".." + Name); }, "Too many dots case");
        }

        [Test]
        public void AddByNullDotExpressionCausesExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(()=> { grammar.SymbolProperties.Add(null); });
        }
    }
}
