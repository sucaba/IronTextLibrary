using IronText.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Reflection.Serialization
{
    [TestFixture]
    public class ProductionTextMatcherTest
    {
        private IProductionTextMatcher sut;

        [SetUp]
        public void SetUp()
        {
            this.sut = new GrammarElementMatcher();
        }
    }
}
