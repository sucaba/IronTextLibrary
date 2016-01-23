using IronText.Reflection;
using IronText.Semantics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Semantics
{
    // INH Equivalence Class (EC) rules:
    // 1) Different attribute names within the same EC cannot belong to the same grammar symbol.
    //    EC stack node can contain only single value while 2 INH attributes can have different values.
    // 2) INH attributes belong to the same EC if there is at least one copy rule between them and they
    //    are not violating rule 1.
    // 3) Copy rules between attributes in the same EC will not be executed in runtime 
    //    because they are not needed.
    // 4) For implementation simplicity EC stacks are synchronized with a parsing stack.
    [TestFixture]
    public class InheritedAttributeECPartitionTest
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

            // [ x, x + 1, y ]
            // E.Env => EC1_stack[-1] // last
            grammar.BuildIndexes();

            Assert.AreEqual(2, grammar.InheritedProperties.Count);

            var sut = new InheritedAttributeECPartition(grammar);
            Assert.AreEqual(1, sut.ECs.Count);
        }

        [Test]
        public void Test()
        {
            var grammar = new Grammar
            {
                StartName = "S",
                Productions =
                {
                    "S : E E E",
                    "E : 't'"
                },
                InheritedProperties =
                {
                    // Global inherited attribute
                    { "S", "Env"  },
                    { "S", "Env2" }
                }
            };

            var prod = grammar.Productions.Find("S : E E E");

            // State push
            prod.Semantics.Add(
                    new SemanticVariable("Env", 0),
                    new [] { new SemanticReference("Env") },
                    (int env) => env + 1);

            // Reuse value from stack with offset -2 (-1 is stack top)
            prod.Semantics.Add(
                    new SemanticVariable("Env", 1),
                    new SemanticReference("Env"));

            // State push
            // S.Env, S.Env2 are in different ECs according to the rule #1
            prod.Semantics.Add(
                    new SemanticVariable("Env", 2),
                    new SemanticReference("Env2"));

            // [ x, x + 1, y ]
            // E.Env => EC1_stack[-1] // last
            grammar.BuildIndexes();

            Assert.AreEqual(3, grammar.InheritedProperties.Count);

            var sut = new InheritedAttributeECPartition(grammar);
            Assert.AreEqual(2, sut.ECs.Count);
        }
    }
}
