using System.Diagnostics;
using System.Text;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class BinaryDecisionTreeBuilderTest
    {
        private readonly DecisionTreePlatformInfo platformInfo = 
                            new DecisionTreePlatformInfo(
                                    maxLinearCount:        3,
                                    branchCost:            7,
                                    switchCost:            3,
                                    maxSwitchElementCount: 1024,
                                    minSwitchDensity:      0.5);
        
        [Test]
        public void Test0()
        {
            var intArrows = 
                new []
                {
                    new IntArrow<int>(0, 10),
                    new IntArrow<int>(2, 20),
                    new IntArrow<int>(5, 10, 30),
                };

            const int DefaultValue = -1;

            var target = new BinaryDecisionTreeBuilder(DefaultValue, platformInfo);
            var node = target.Build(intArrows);
            PrintProgram(node);

            Assert.AreEqual(DefaultValue, Eval(node, -1));
            Assert.AreEqual(10, Eval(node, 0));
            Assert.AreEqual(DefaultValue, Eval(node, 1));
            Assert.AreEqual(20, Eval(node, 2));
            Assert.AreEqual(DefaultValue, Eval(node, 3));
            Assert.AreEqual(DefaultValue, Eval(node, 4));
            Assert.AreEqual(30, Eval(node, 5));
            Assert.AreEqual(30, Eval(node, 10));
            Assert.AreEqual(DefaultValue, Eval(node, 11));
        }

        [Test]
        public void SingleValueInterval()
        {
            var intArrows = 
                new []
                {
                    new IntArrow<int>(0, 10),
                };

            const int DefaultValue = -1;

            var target = new BinaryDecisionTreeBuilder(DefaultValue, platformInfo);
            var node = target.Build(intArrows);
            PrintProgram(node);

            Assert.AreEqual(DefaultValue, Eval(node, -1));
            Assert.AreEqual(10, Eval(node, 0));
            Assert.AreEqual(DefaultValue, Eval(node, 1));
        }

        [Test]
        public void SingleInterval()
        {
            var intArrows = 
                new []
                {
                    new IntArrow<int>(0, 5, 10),
                };

            const int DefaultValue = -1;

            var target = new BinaryDecisionTreeBuilder(DefaultValue, platformInfo);
            var node = target.Build(intArrows);
            PrintProgram(node);

            Assert.AreEqual(DefaultValue, Eval(node, -1));
            Assert.AreEqual(10, Eval(node, 0));
            Assert.AreEqual(10, Eval(node, 3));
            Assert.AreEqual(10, Eval(node, 5));
            Assert.AreEqual(DefaultValue, Eval(node, 6));
        }

        [Conditional("DEBUG")]
        private void PrintProgram(Decision nodes)
        {
            StringBuilder output = new StringBuilder();
            nodes.Accept(new DecisionProgramWriter(output));
            Debug.WriteLine(output);
        }

        private int Eval(Decision node, int input)
        {
            return node.Decide(input);
        }
    }
}
