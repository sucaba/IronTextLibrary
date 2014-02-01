using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronText.Diagnostics;
using IronText.Framework;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    // TODO: Test resolving reduce-reduce conflicts
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
        public void ResolvingAmbiguities()
        {
            using (var interp = new Interpreter<AmbiguousCalculator>())
            {
                interp.Parse("2+8/2");
                Assert.AreEqual(6, interp.Context.Result.Value);

                interp.Parse("8/4/2");
                Assert.AreEqual(1, interp.Context.Result.Value);

                interp.Parse("2+3"); // can be interpreted as a "2 * (+3)"
                Assert.AreEqual(5, interp.Context.Result.Value);

                // Check that implicit multiplication works
                interp.Parse("2 3"); 
                Assert.AreEqual(6, interp.Context.Result.Value);

                // A lot of ambiguities:
                interp.Parse("1+-+6/3"); 
                Assert.AreEqual(-1, interp.Context.Result.Value);
#if false
                using (var g = new GvGraphView("expr.tmp.gv"))
                {
                    interp.BuildTree("1+-+6/3").WriteGraph(g, interp.Grammar, true);
                }
#endif
            }
        }

        [Language(LanguageFlags.AllowNonDeterministic)]
        [ParserGraph("SAdBLang.gv")]
        public class SAdBLang
        {
            public bool IsFinalRuleCalledLast = false;
            public bool WasMergeCalled = false;

            [Produce]
            public void All(A a)
            {
                IsFinalRuleCalledLast = true;
                Debug.WriteLine("void -> A // report result"); 
            }

            [Produce("d")]
            public A Ad() 
            {  
                IsFinalRuleCalledLast = false;
                Debug.WriteLine("A -> d"); 
                return null;
            } 

            [Produce]
            public A A(B b) 
            {  
                IsFinalRuleCalledLast = false;
                Debug.WriteLine("A -> B"); return null; 
            } 

            [Produce("d")]
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
#if false
        [DescribeParserStateMachine("NondeterministicCalc3.info")]
        [ScannerDocument("NondeterministicCalc3.scan")]
        [ScannerGraph("NondeterministicCalc3_Scanner.gv")]
#endif
        public class AmbiguousCalculator
        {
            [Outcome]
            public Expr Result { get; set; }

            [Produce(null, "+", null)]
            public Expr Plus(Expr x, Expr y)
            {
                return new Expr(x.Value + y.Value, precedence: 1, assoc: Associativity.Left);
            }

            [Produce("+", null)]
            public Expr UnaryPlus(Expr x)
            {
                return new Expr(x.Value, precedence: 3, assoc: Associativity.Left);
            }

            [Produce("-", null)]
            public Expr UnaryMinus(Expr x)
            {
                return new Expr(-x.Value, precedence: 3, assoc: Associativity.Right);
            }

            [Produce(null, "*", null)]    // explicit multiplication
            [Produce]                     // implicit multiplication
            public Expr Mult(Expr x, Expr y)
            {
                return new Expr(x.Value * y.Value, precedence: 2, assoc: Associativity.Left);
            }

            [Produce(null, "/", null)]
            public Expr Div(Expr x, Expr y)
            {
                return new Expr(x.Value / y.Value, precedence: 2, assoc: Associativity.Left);
            }

            [Produce]
            public Expr Constant(double value)
            {
                return new Expr(value, precedence: 10);
            }
            
            // Resolve operator precedence problems in runtime
            [Merge]
            public Expr MergeExpr(Expr reduceFirst, Expr shiftFirst)
            {
                // High precedence rule should be reduced first: left child in tree
                // and low precedence rule should be reduced last: parent in tree.
                // Merge method should return tree with a lowest precedence.
                if (reduceFirst.Precedence < shiftFirst.Precedence)
                {
                    return reduceFirst;
                }
                if (reduceFirst.Precedence > shiftFirst.Precedence)
                {
                    return shiftFirst;
                }

                switch (reduceFirst.Associativity)
                {
                    case Associativity.Left: return reduceFirst;
                    case Associativity.Right: return shiftFirst;
                    default:
                        throw new InvalidOperationException("Unable to resolve ambiguity.");
                }
            }

            [Match("digit+ ('.' digit*)? | '.' digit+")]
            public double Real(string text) { return double.Parse(text); }

            [Match("blank+")]
            public void Blank() {}
        }

        public class Expr
        {
            public readonly int Precedence;
            public readonly double Value;
            public readonly Associativity Associativity;

            public Expr(double value, int precedence, Associativity assoc = Associativity.Left)
            {
                this.Value = value;
                this.Precedence = precedence;
                this.Associativity = assoc;
            }

            public override string ToString()
            {
                return string.Format(
                    "Expr(value={0}, prec={1}, assoc={2})",
                    Value,
                    Precedence,
                    Enum.GetName(typeof(Associativity), Associativity));
            }
        }
    }
}
