using IronText.Reflection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Reflection.Serialization
{
    [TestFixture]
    public class ProductionNameResolverTest
    {
        private IProductionNameResolver sut;
        private Mock<INameResolver<Symbol>> symbolNameResolverMock;
        private Mock<IAddOnlyCollection<Production>> collectionMock;
        private Mock<IProductionTextMatcher> matcherMock;

        [SetUp]
        public void SetUp()
        {
            this.symbolNameResolverMock = new Mock<INameResolver<Symbol>>(MockBehavior.Strict); 
            this.collectionMock         = new Mock<IAddOnlyCollection<Production>>(MockBehavior.Strict); 
            this.matcherMock            = new Mock<IProductionTextMatcher>(MockBehavior.Strict);
            this.sut                    = new ProductionNameResolver(symbolNameResolverMock.Object, collectionMock.Object, matcherMock.Object);
        }

        [Test]
        public void CreateMissingProductionTest()
        {
            symbolNameResolverMock
                .Setup(sr => sr.Resolve("A", true))
                .Returns(new Symbol("A"));
            symbolNameResolverMock
                .Setup(sr => sr.Resolve("B", true))
                .Returns(new Symbol("B"));
            symbolNameResolverMock
                .Setup(sr => sr.Resolve("C", true))
                .Returns(new Symbol("C"));
            collectionMock
                .Setup(coll => coll.GetEnumerator())
                .Returns((IEnumerator<Production>)new List<Production>().GetEnumerator());
            collectionMock.Setup(coll => coll.Add(It.IsAny<Production>()));
            var prod = sut.Resolve("A = B C", createMissing: true);
            Assert.IsNotNull(prod);
            Assert.AreEqual("A", prod.Outcome.Name);
            symbolNameResolverMock.VerifyAll();
            collectionMock.VerifyAll();
            matcherMock.VerifyAll();
        }

        [Test]
        public void FindExistingProductionTest([Values(false, true)]bool createMissing)
        {
            var existingProduction = new Production(new Symbol("A"), new [] { new Symbol("B"), new Symbol("C") });

            matcherMock
                .Setup(m => m.Match(existingProduction, It.IsAny<ProductionSketch>()))
                .Returns(true);
            collectionMock
                .Setup(coll => coll.GetEnumerator())
                .Returns((IEnumerator<Production>)new List<Production> { existingProduction }.GetEnumerator());
            var prod = sut.Resolve("A = B C", createMissing: createMissing);
            Assert.AreSame(existingProduction, prod);
            symbolNameResolverMock.VerifyAll();
            collectionMock.VerifyAll();
            matcherMock.VerifyAll();
        }
    }
}
