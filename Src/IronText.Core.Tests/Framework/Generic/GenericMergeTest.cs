using IronText.Framework;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;
using System;
using static IronText.Tests.Framework.Generic.GrammarsUnderTest;

namespace IronText.Tests.Framework.Generic
{
    [TestFixture]
    public class GenericMergeTest
    {
        [Test]
        public void TrivialMerge()
        {
            var r = Language.Parse(new TrivialMergeLanguage(), "a").Result;
            Assert.That(r, Has.Count.EqualTo(1));
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
            }
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph(nameof(AmbiguousCalculator) + "0.gv")]
        [DescribeParserStateMachine(nameof(AmbiguousCalculator) + "0.info")]
        public class AmbiguousCalculator
        {
            private Expr _result;

            [Outcome]
            public Expr Result
            {
                get { return _result; }
                set
                {
                    _result = value;
                }
            }

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
                // Between two choose the one which has lower priority at the root
                // because it means that higher priorirty was reduced first and is
                // a child in reduction tree.
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
