using System.Diagnostics;
using System.Text;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class DecisionTreeBuilderTest
    {
        private readonly DecisionTreePlatformInfo platformInfo = 
                            new DecisionTreePlatformInfo(
                                    maxLinearCount:        3,
                                    branchCost:            3,
                                    switchCost:            7,
                                    maxSwitchElementCount: 1024,
                                    minSwitchDensity:      0.5);
        [Test]
        public void TestSameActionUnification()
        {
            var frequency = new UniformIntFrequency(new IntInterval(-100, 100));

            const int DefaultValue = -100;
            var elementToAction = new MutableIntMap<int>();
            elementToAction.DefaultValue = DefaultValue;
            elementToAction.Set(new IntArrow<int>(1, 1));
            elementToAction.Set(new IntArrow<int>(3, 49, 1));
            elementToAction.Set(new IntArrow<int>(51, 1));
            elementToAction.Set(new IntArrow<int>(54, 100, 1));

            var target = new DecisionTreeBuilder(platformInfo);
            var bounds = new IntInterval(0, 1000);
            var node = target.Build(elementToAction, bounds, frequency);
            PrintProgram(node);
        }

        [Test]
        public void TestElementaryChecks()
        {
            const int DefaultValue = -100;
            var elementToAction = new MutableIntMap<int>();
            elementToAction.DefaultValue = DefaultValue;
            var bounds = new IntInterval(0, 9);
            var frequency = new UniformIntFrequency(bounds);
            elementToAction.Set(new IntArrow<int>(2, 100));
            elementToAction.Set(new IntArrow<int>(5, 200));

            var target = new DecisionTreeBuilder(platformInfo);
            var node = target.Build(elementToAction, bounds, frequency);
            PrintProgram(node, target.DefaultActionDecision);
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

            var target = new DecisionTreeBuilder(platformInfo);
            var bounds = new IntInterval(int.MinValue, int.MaxValue);
            var node = target.Build(elementToAction, bounds, frequency);
            PrintProgram(node, target.DefaultActionDecision);

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

        [Conditional("DEBUG")]
        private void PrintProgram(Decision nodes, Decision defaultDecision = null)
        {
            if (defaultDecision == null)
            {
                defaultDecision = new ActionDecision(-1);
            }

            StringBuilder output = new StringBuilder();
            var writer = new DecisionProgramWriter(output);
            writer.Build(nodes, defaultDecision);
            Debug.WriteLine(output);
        }

        private int Eval(Decision node, int input)
        {
            return node.Decide(input);
        }
    }
}
