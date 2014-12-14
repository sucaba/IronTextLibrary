using IronText.Framework;
using IronText.Runtime;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Runtime.Deterministic
{
    [TestFixture]
    public class DeterministicParsingTest
    {
        [Test]
        public void StartEpsilonProductionIsInvoked()
        {
            var contextMock = new Mock<IEpsilonLanguage>();
            Language.Parse(contextMock.Object, "");
            contextMock.Verify(c => c.Start());
        }

        [Language]
        public interface IEpsilonLanguage
        {
            [Produce]
            void Start();
        }
    }
}
