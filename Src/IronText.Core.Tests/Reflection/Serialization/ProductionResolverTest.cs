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
    public class ProductionResolverTest
    {
        private IProductionResolver sut;
        private Mock<ISymbolResolver> symbolNameResolverMock;
        private Mock<IAddOnlyCollection<Production>> collectionMock;
        private Mock<IProductionTextMatcher> matcherMock;

        [SetUp]
        public void SetUp()
        {
            this.symbolNameResolverMock = new Mock<ISymbolResolver>(MockBehavior.Strict); 
            this.collectionMock         = new Mock<IAddOnlyCollection<Production>>(MockBehavior.Strict); 
            this.matcherMock            = new Mock<IProductionTextMatcher>(MockBehavior.Strict);
            this.sut                    = new ProductionResolver(symbolNameResolverMock.Object, collectionMock.Object, matcherMock.Object);
        }

        [Test]
        public void CreateInstanceTest()
        {
            symbolNameResolverMock
                .Setup(sr => sr.Resolve("A"))
                .Returns(new Symbol("A"));
            symbolNameResolverMock
                .Setup(sr => sr.Resolve("B"))
                .Returns(new Symbol("B"));
            symbolNameResolverMock
                .Setup(sr => sr.Resolve("C"))
                .Returns(new Symbol("C"));
            collectionMock
                .Setup(c => c.Add(It.IsAny<Production>()));
            var prod = sut.Create("A = B C");
            Assert.IsNotNull(prod);
            Assert.AreEqual("A", prod.Outcome.Name);
            symbolNameResolverMock.VerifyAll();
            collectionMock
                .Verify(c => c.Add(prod));
            matcherMock.VerifyAll();
        }

        [Test]
        public void FindExistingProductionTest()
        {
            var existingProduction = new Production(new Symbol("A"), new [] { new Symbol("B"), new Symbol("C") });

            matcherMock
                .Setup(m => m.Match(existingProduction, It.IsAny<ProductionSketch>()))
                .Returns(true);
            collectionMock
                .Setup(coll => coll.GetEnumerator())
                .Returns(new List<Production> { existingProduction }.GetEnumerator());
            var prod = sut.Find("A = B C");
            Assert.AreSame(existingProduction, prod);
            symbolNameResolverMock.VerifyAll();
            collectionMock.VerifyAll();
            matcherMock.VerifyAll();
        }

        [Test]
        public void FindMissingProductionTest()
        {
            collectionMock
                .Setup(coll => coll.GetEnumerator())
                .Returns(new List<Production>().GetEnumerator());
            var prod = sut.Find("A = B C");
            Assert.IsNull(prod);
            symbolNameResolverMock.VerifyAll();
            collectionMock.VerifyAll();
            matcherMock.VerifyAll();
        }
    }
}
