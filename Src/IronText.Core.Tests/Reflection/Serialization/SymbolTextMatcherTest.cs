using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Reflection.Serialization
{
    [TestFixture]
    public class SymbolTextMatcherTest
    {
        private ISymbolTextMatcher sut;

        [SetUp]
        public void SetUp()
        {
            this.sut = new GrammarElementMatcher();
        }

        [Test]
        public void SymbolMatch()
        {
            Assert.IsTrue(sut.MatchSymbol(new Symbol("foo"), "foo"));
        }
    }
}
