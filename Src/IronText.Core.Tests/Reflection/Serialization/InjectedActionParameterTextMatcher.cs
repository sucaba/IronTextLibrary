using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Reflection.Serialization
{
    [TestFixture]
    public class InjectedActionParameterTextMatcherTest
    {
        private IInjectedActionParameterTextMatcher sut;

        [SetUp]
        public void SetUp()
        {
            this.sut = new GrammarElementMatcher();
        }

        [Test]
        public void InjecttedActionParametterMatch()
        {
            Assert.IsTrue(sut.Match(new InjectedActionParameter("foo"), "?foo"));
        }
    }
}
