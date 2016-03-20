using IronText.Reflection;
using IronText.Semantics;
using NUnit.Framework;
using System.Linq;

namespace IronText.Tests.Semantics
{
    // INH Equivalence Class (EC) rules:
    // 1) Different attribute belonging to the same grammar symbol cannot be in the same EC.
    //    EC stack node can contain only single value while 2 INH attributes can have different values.
    // 2) INH attributes belong to the same EC if there is at least one copy rule between them and they
    //    are not violating directly or indirectly rule #1.
    // 3) Copy rules between attributes in the same EC will not be executed in runtime 
    //    because they are not needed.
    // 4) For implementation simplicity EC stacks are synchronized with a parsing stack.
    [TestFixture]
    public class InheritedAttributeECCollectionTest
    {
        [Test]
        public void CopyRuleCausesInhAttrsToBeInTheSameECTest()
        {
            var grammar = new Grammar
            {
                StartName = "S",
                Productions =
                {
                    "S : E",
                    "E : 'x'"
                },
                InheritedProperties =
                {
                    // Global inherited attribute
                    { "S", "Env" },
                }
            };

            var prod = grammar.Productions.Find("S : E");

            // Add copy rule
            prod.Semantics.Add(
                    new SemanticVariable("Env", 0),
                    new SemanticReference("Env"));

            grammar.BuildIndexes();

            Assert.AreEqual(2, grammar.InheritedProperties.Count);

            var sut = new InheritedPropertyECCollection(grammar);
            Assert.AreEqual(1, sut.Count);
        }

        [Test]
        public void SameSymbolInhAttrsPreventECPartitioningTest()
        {
            var grammar = new Grammar
            {
                StartName = "S",
                Productions =
                {
                    "S : E E",
                    "E : 't'"
                },
                InheritedProperties =
                {
                    // Global inherited attribute
                    { "S", "Env"  },
                    { "S", "Env2" }
                }
            };

            var prod = grammar.Productions.Find("S : E E");

            prod.Semantics.Add(
                    new SemanticVariable("Env", 0),
                    new SemanticReference("Env"));

            // S.Env, S.Env2 are in different ECs according to the rule #1
            prod.Semantics.Add(
                    new SemanticVariable("Env", 1),
                    new SemanticReference("Env2"));

            grammar.BuildIndexes();

            var sut = new InheritedPropertyECCollection(grammar);
            CollectionAssert.AreEquivalent(
                grammar.InheritedProperties.Select(inh => new[] { inh.Index }),
                sut);
        }
    }
}
