using System.Diagnostics;
using System.Text;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class DecisionTreeBuilderTest
    {
        [Test]
        public void TestSameActionUnification()
        {
            var frequency = new UniformIntFrequency(new IntInterval(-100, 100));

            const int DefaultValue = -1;
            var elementToAction = new MutableIntMap<int>();
            elementToAction.DefaultValue = DefaultValue;
            elementToAction.Set(new IntArrow<int>(1, 1));
            elementToAction.Set(new IntArrow<int>(2, 49, 1));
            elementToAction.Set(new IntArrow<int>(50, 1));
            elementToAction.Set(new IntArrow<int>(51, 100, 1));

            var target = new DecisionTreeBuilder(-100);
            var bounds = new IntInterval(int.MinValue, int.MaxValue);
            var node = target.BuildBalanced(elementToAction, bounds, frequency);
            PrintProgram(node);
        }

        [Test]
        public void TestBalanced()
        {
            var frequency = new MutableIntFrequency();
            frequency.DefaultValue = 0.0000001;
            frequency.Set(new IntArrow<double>(1, 520.0));
            frequency.Set(new IntArrow<double>(2, 49, 3.0));
            frequency.Set(new IntArrow<double>(50, 236.0));
            frequency.Set(new IntArrow<double>(51, 100, 2.0));

            const int DefaultValue = -1;
            var elementToAction = new MutableIntMap<int>();
            elementToAction.DefaultValue = DefaultValue;
            elementToAction.Set(new IntArrow<int>(1, 1));
            elementToAction.Set(new IntArrow<int>(2, 49, 2));
            elementToAction.Set(new IntArrow<int>(50, 3));
            elementToAction.Set(new IntArrow<int>(51, 100, 4));

            var target = new DecisionTreeBuilder(-100);
            var bounds = new IntInterval(int.MinValue, int.MaxValue);
            var node = target.BuildBalanced(elementToAction, bounds, frequency);
            PrintProgram(node);

            Assert.AreEqual(-1, node.Decide(int.MinValue) );
            Assert.AreEqual(-1, node.Decide(0) );
            Assert.AreEqual(1, node.Decide(1) );
            Assert.AreEqual(2, node.Decide(2) );
            Assert.AreEqual(2, node.Decide(49) );
            Assert.AreEqual(3, node.Decide(50) );
            Assert.AreEqual(4, node.Decide(51) );
            Assert.AreEqual(4, node.Decide(100) );
            Assert.AreEqual(-1, node.Decide(200) );
            Assert.AreEqual(-1, node.Decide(bounds.Last) );
        }

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

            var target = new DecisionTreeBuilder(DefaultValue);
            var node = target.BuildBinaryTree(intArrows);
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

            var target = new DecisionTreeBuilder(DefaultValue);
            var node = target.BuildBinaryTree(intArrows);
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

            var target = new DecisionTreeBuilder(DefaultValue);
            var node = target.BuildBinaryTree(intArrows);
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
            nodes.PrintProgram(new DecisionProgramWriter(output));
            Debug.WriteLine(output);
        }

        private int Eval(Decision node, int input)
        {
            return node.Decide(input);
        }
    }
}
