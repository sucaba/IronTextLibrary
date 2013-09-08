using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronText.Diagnostics;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class MergeTest
    {
        [Test]
        public void Test()
        {
            var context = new SAdBLang();
            using (var interp = new Interpreter<SAdBLang>(context))
            {
                interp.Parse("d");
                Assert.IsTrue(context.IsFinalRuleCalledLast);
                Assert.IsTrue(context.WasMergeCalled);

                // with SPPF producer
                var sppf = interp.BuildTree("d");

                using (var graph = new GvGraphView(typeof(SAdBLang).Name + "_sppf.gv"))
                {
                    sppf.WriteGraph(graph, interp.Grammar, true);
                }
            }
        }

        [Test]
        public void CanProduceMultipleResults()
        {
            var lang = Language.Get(typeof(NondeterministicCalc2));
            using (var interp = new Interpreter<NondeterministicCalc2>(
                    new NondeterministicCalc2(mergeToOlder: true)))
            {
                interp.Parse("3^3^3");
                Assert.AreEqual(1, interp.Context.Results.Count, "Results should be merged");
                Assert.AreEqual(19683.0, interp.Context.Results[0]);

                interp.Context = new NondeterministicCalc2(mergeToOlder: false);
                interp.Parse("3^3^3");
                Assert.AreEqual(1, interp.Context.Results.Count, "Results should be merged");
                Assert.AreEqual(7625597484987.0, interp.Context.Results[0]);
            }
        }

        [Language(LanguageFlags.AllowNonDeterministic)]
        [ParserGraph("SAdBLang.gv")]
        public class SAdBLang
        {
            public bool IsFinalRuleCalledLast = false;
            public bool WasMergeCalled = false;

            [Parse]
            public void All(A a)
            {
                IsFinalRuleCalledLast = true;
                Debug.WriteLine("void -> A // report result"); 
            }

            [Parse("d")]
            public A Ad() 
            {  
                IsFinalRuleCalledLast = false;
                Debug.WriteLine("A -> d"); 
                return null;
            } 

            [Parse]
            public A A(B b) 
            {  
                IsFinalRuleCalledLast = false;
                Debug.WriteLine("A -> B"); return null; 
            } 

            [Parse("d")]
            public B Bd() 
            {  
                IsFinalRuleCalledLast = false;
                Debug.WriteLine("B -> d");
                return null; 
            }  

            [Merge]
            public A MergeA(A a1, A a2)
            {
                IsFinalRuleCalledLast = false;
                WasMergeCalled = true;

                Debug.WriteLine("Merging A action");
                return a2;
            }
        }

        public interface A { }
        public interface B { }

        [Language(LanguageFlags.AllowNonDeterministic)]
        public class NondeterministicCalc2
        {
            public readonly List<double> Results = new List<double>();

            private readonly bool mergeToOlder;

            public NondeterministicCalc2(bool mergeToOlder)
            {
                this.mergeToOlder = mergeToOlder;
            }

            [Parse]
            public void AddResult(double e)
            {
                Debug.WriteLine("AddResult({0})", e);
                Results.Add(e);
            }

            [Parse(null, "^", null)]
            public double Pow(double e1, double e2) { return Math.Pow(e1, e2); }

            [Parse("3")]
            public double Number() { return 3; }

            [Merge]
            public double Merge(double oldValue, double newValue)
            {
                if (mergeToOlder)
                {
                    return oldValue;
                }
                else
                {
                    return newValue;
                }
            }
        }
    }
}
