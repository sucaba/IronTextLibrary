using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Diagnostics;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class SppfProducerTest
    {
        [Test]
        public void TestRecursiveTree()
        {
            using (var interp = new Interpreter<RecursiveTree>())
            {
                var sppf = interp.BuildTree("a");
                sppf.WriteIndented(interp.Grammar, Console.Out, 0);
                using (var graph = new GvGraphView(typeof(RecursiveTree).Name + "_sppf_amb.gv"))
                {
                    sppf.WriteGraph(graph, interp.Grammar, true);
                }
            }
        }

        [Test]
        public void TestShareBranchNodesWithTree()
        {
            using (var interp = new Interpreter<ShareBranchNodesWithTree>())
            {
                var sppf = interp.BuildTree("bb");
                sppf.WriteIndented(interp.Grammar, Console.Out, 0);
                using (var graph = new GvGraphView(typeof(ShareBranchNodesWithTree).Name + "_sppf_amb.gv"))
                {
                    sppf.WriteGraph(graph, interp.Grammar, true);
                }
            }
        }

        [Test]
        public void TestAmbiguousWithEpsilonTree()
        {
            using (var interp = new Interpreter<RightNullableWithTree>())
            {
                var sppf = interp.BuildTree("aaab");
                sppf.WriteIndented(interp.Grammar, Console.Out, 0);
                using (var graph = new GvGraphView(typeof(RightNullableWithTree).Name + "_sppf_amb.gv"))
                {
                    sppf.WriteGraph(graph, interp.Grammar, true);
                }
            }
        }

        [Test]
        public void TestAmbiguousTree()
        {
            var lang = Language.Get(typeof(NondeterministicCalcForTree));

            using (var interp = new Interpreter<NondeterministicCalcForTree>())
            {
                var sppf = interp.BuildTree("3^3^3");
                sppf.WriteIndented(lang.Grammar, Console.Out, 0);
                using (var graph = new GvGraphView(typeof(NondeterministicCalcForTree).Name + "_sppf_amb.gv"))
                {
                    sppf.WriteGraph(graph, lang.Grammar);
                }

                var allNodes = sppf.Flatten().ToArray();

                var NUM = lang.Identify("3");
                var numNodes = allNodes.Where(n => n.Id == NUM).Distinct(IdentityComparer.Default).ToArray();
                Assert.AreEqual(3, numNodes.Length, "Leaf SPPF nodes should be shared");

                var POW = lang.Identify("^");
                var powNodes = allNodes.Where(n => n.Id == POW).Distinct(IdentityComparer.Default).ToArray();
                Assert.AreEqual(2, powNodes.Length, "Leaf SPPF nodes should be shared");
            }
        }

        class IdentityComparer : IEqualityComparer<object>
        {
            public static readonly IdentityComparer Default = new IdentityComparer();

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }

        [Test]
        public void TestDeterministicTree()
        {
            using (var interp = new Interpreter<NondeterministicCalcForTree>())
            {
                var sppf = interp.BuildTree("3^3");
                sppf.WriteIndented(interp.Grammar, Console.Out, 0);
                using (var graph = new GvGraphView(typeof(NondeterministicCalcForTree).Name + "_sppf_det.gv"))
                {
                    sppf.WriteGraph(graph, interp.Grammar);
                }
            }
        }

        [Test]
        public void TestDeterministicTree0()
        {
            using (var interp = new Interpreter<NondeterministicCalcForTree>())
            {
                var sppf = interp.BuildTree("3");
                sppf.WriteIndented(interp.Grammar, Console.Out, 0);
                using (var graph = new GvGraphView(typeof(NondeterministicCalcForTree).Name + "0_sppf_det.gv"))
                {
                    sppf.WriteGraph(graph, interp.Grammar);
                }
            }
        }

        [Language(LanguageFlags.ForceNonDeterministic)]
        [ParserGraph("NondeterministicCalcForTree.gv")]
        public class NondeterministicCalcForTree
        {
            public readonly List<double> Results = new List<double>();

            [Parse]
            public void AddResult(double e) { Results.Add(e); }

            [Parse(null, "^", null)]
            public double Pow(double e1, double e2) { return Math.Pow(e1, e2); }

            [Parse("3")]
            public double Number() { return 3; }
        }

        [Language(LanguageFlags.ForceNonDeterministic)]
        [ParserGraph("RightNullableWithTree.gv")]
        public interface RightNullableWithTree
        {
            [Parse(null, null, "a", "b")]
            void S(B b, D d);

            [Parse("a", null, "a", "d")]
            void S(D d);

            [Parse("a")]
            D D(A a, B b);

            [Parse("a")]
            A A(B b1, B b2);

            [Parse]
            A A();

            [Parse]
            B B();
        }

        [Language]
        [ParserGraph("RecursiveTree.gv")]
        public interface RecursiveTree
        {
            [Parse]
            void Start(S s);

            [Parse]
            S Sdouble(S s1, S s2);

            [Parse("a")]
            S Sa();

            [Parse]
            S Sempty();
        }

        [Language]
        [ParserGraph("ShareBranchNodesWithTree.gv")]
        public interface ShareBranchNodesWithTree
        {
            [Parse]
            void Start(S s);

            [Parse("b")]
            S Sdouble(B b, S s1, S s2);

            [Parse("a")]
            S Sa();

            [Parse]
            S Sempty();

            [Parse]
            B Bempty();
        }

        public interface D {}
        public interface A {}
        public interface B {}
        public interface S {}
    }
}
