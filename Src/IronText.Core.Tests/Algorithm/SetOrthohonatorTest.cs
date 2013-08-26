using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class SetOrthohonatorTest
    {
        static IntSetType IntSet = SparseIntSetType.Instance;

        // given set alphabet
        const int A = 0;
        const int B = 1;
        const int C = 2;
        const int D = 3;
        const int E = 4;
        const int F = 4;
        const int G = 4;
        private IntSet[] inputSets;
        private List<IntSet> outputSets;

        [Test]
        public void TestOrthohonate()
        {
            GivenInputSets(
                IntSet.Of( A, B, C ),
                IntSet.Of( B, C, D ),
                IntSet.Of( E ),         // unary set should remain unchaged
                IntSet.Of( A ),         // unary set will not repeat more than once
                IntSet.Of( C, B ),      // duplicate set which will be in output
                IntSet.Of(  )           // empty set will be ignored
                );

            SetOrthohonator.Orthohonate(outputSets);

            Console.WriteLine("Alphabet:" + inputSets.Aggregate((x,y) => IntSet.Of(x.Union(y))));
            Console.WriteLine(string.Join(", ", outputSets));
            Assert.AreEqual(4, outputSets.Count);
            SetsShouldNotHaveCommonItems(outputSets);
            EachResultSetIsSubsetOfSomeInputSet(outputSets);
        }

        public void TestOrthohonateAdd()
        {
            GivenInputSets(
                IntSet.Of( A, B, C ),
                IntSet.Of( B, C, D ),
                IntSet.Of( E ),         // unary set should remain unchaged
                IntSet.Of( A ),         // unary set will not repeat more than once
                IntSet.Of( C, B ),      // duplicate set which will be in output
                IntSet.Of(  )           // empty set will be ignored
                );

            outputSets = new List<IntSet>();

            foreach (var item in inputSets)
            {
                SetOrthohonator.OrthohonalAdd(outputSets, item);
            }

            Console.WriteLine("Alphabet:" + inputSets.Aggregate((x,y) => IntSet.Of(x.Union(y))));
            Console.WriteLine(string.Join(", ", outputSets));
            Assert.AreEqual(4, outputSets.Count);
            SetsShouldNotHaveCommonItems(outputSets);
            EachResultSetIsSubsetOfSomeInputSet(outputSets);
        }

        private void EachResultSetIsSubsetOfSomeInputSet(List<IntSet> outputSets)
        {
            foreach (var set in outputSets)
            {
                Assert.IsTrue(inputSets.Any(s0 => s0.IsSupersetOf(set)));
            }
        }

        private void GivenInputSets(params IntSet[] sets)
        {
            inputSets = sets;
            outputSets = sets.ToList();
        }

        private void SetsShouldNotHaveCommonItems(List<IntSet> outputSets)
        {
            int len = outputSets.Count;
            for (int i = 0; i != len; ++i)
            {
                IntSet x = outputSets[i];
                for (int j = i + 1; j < len; ++j)
                {
                    IntSet y = outputSets[j];
                    CollectionAssert.IsEmpty(x.Intersect(y));
                }
            }
        }
    }
}
