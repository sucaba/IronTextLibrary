using IronText.Reflection;
using NUnit.Framework;
using System;

namespace IronText.Tests.Reflection
{
    [TestFixture]
    public class InjectedActionParameterTest
    {
        private const int ValidPosition   = 1;
        private const int InvalidPosition = 3;
        private readonly Production production = new Production(new Symbol("outcome"), new [] { new Symbol("arg0"),  new Symbol("arg1") });

        [Test]
        public void ConstructintTest()
        {
            var target = new InjectedActionParameter(production, ValidPosition);
            Assert.AreSame(production, target.Production);
            Assert.AreEqual(ValidPosition, target.Position);
        }

        [Test]
        public void ConstructionWithNullProductionCausesExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(()=> new InjectedActionParameter(null, ValidPosition));
        }

        [Test]
        public void ConstructionWithInvalidPositionCausesExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=> new InjectedActionParameter(production, -1));
            Assert.Throws<ArgumentOutOfRangeException>(()=> new InjectedActionParameter(production, InvalidPosition));
        }
    }
}
