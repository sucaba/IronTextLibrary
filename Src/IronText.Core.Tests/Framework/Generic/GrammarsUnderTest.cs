using IronText.Framework;
using IronText.Reflection;
using System;
using System.Collections.Generic;

namespace IronText.Tests.Framework.Generic
{
    public static class GrammarsUnderTest
    {
        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("LeftRecursion0.gv")]
        [DescribeParserStateMachine("LeftRecursion0.info")]
        public interface LeftRecursion
        {
            [Produce]
            void All(A s);

            [Produce]
            A S();

            [Produce(null, "a")]
            A S(A before);
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("HiddenLeftRecursion0.gv")]
        [DescribeParserStateMachine("HiddenLeftRecursion0.info")]
        public interface HiddenLeftRecursion
        {
            [Produce]
            void All(S s);

            [Produce]
            S S();

            [Produce(null, null, "a")]
            S S(S s1, S s2);
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph(nameof(TrivialMergeLanguage) + "0.gv")]
        [DescribeParserStateMachine(nameof(TrivialMergeLanguage) + "0.info")]
        public class TrivialMergeLanguage
        {
            public List<string> Result { get; } = new List<string>();

            [Produce]
            public void SetResult(string text)
            {
                Result.Add(text);
            }

            [Produce]
            public string Concat(bool first, bool second) => first + "," + second;

            [Merge]
            public string Merge(string x, string y) => x + "|" + y;

            [Merge]
            public bool Merge(bool x, bool y) => y;

            [Produce]
            public bool X() => false;

            [Produce("a")]
            public bool Xa() => true;
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph(nameof(LeftRecursionWithBottomUpToken) + "0.gv")]
        [DescribeParserStateMachine(nameof(LeftRecursionWithBottomUpToken) + "0.info")]
        public interface LeftRecursionWithBottomUpToken
        {
            [Produce]
            void All(S s);

            [ProduceBottomUp(null, "a")]
            S Add(S s);

            [ProduceBottomUp]
            S Create();
        }

        [Vocabulary]
        public abstract class NondeterministicCalcBase
        {
            public readonly List<double> Results = new List<double>();

            [Produce]
            public void AddResult(double e) { Results.Add(e); }

            [Produce(null, "^", null)]
            public double Pow(double e1, double e2) { return Math.Pow(e1, e2); }

            [Produce("3")]
            public double Number() { return 3; }

            [Merge]
            public double Merge(double x, double y)
            {
                return y;
            }
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph(nameof(NondeterministicCalc) + "0.gv")]
        [DescribeParserStateMachine(nameof(NondeterministicCalc) + "0.info")]
        public class NondeterministicCalc : NondeterministicCalcBase
        {
        }

        [Language(RuntimeOptions.ForceGenericLR)]
        [ParserGraph(nameof(NondeterministicCalcWithAutoBottomUp) + "0.gv")]
        [DescribeParserStateMachine(nameof(NondeterministicCalcWithAutoBottomUp) + "0.info")]
        public class NondeterministicCalcWithAutoBottomUp : NondeterministicCalcBase
        {
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph(nameof(AmbiguousCalculator) + "0.gv")]
        [DescribeParserStateMachine(nameof(AmbiguousCalculator) + "0.info")]
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

        [Language(RuntimeOptions.ForceGeneric)]
        public class TopDownConsumesBottomUpLanguage
        {
            [Produce]
            public void Result(S s) { }

            [ProduceTopDown("b", null, "e")]
            public S TopDown(A a) => null;

            [ProduceBottomUp("a")]
            public A BottomUp() => null;
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [DescribeParserStateMachine(nameof(BottomUpConsumesTopDownLanguage) + ".info")]
        [ParserGraph(nameof(BottomUpConsumesTopDownLanguage) + ".gv")]
        public class BottomUpConsumesTopDownLanguage
        {
            [Produce]
            public void Result(S s) { }

            [ProduceBottomUp("b", null, "e")]
            public S BottomUp(A a) => null;

            [ProduceTopDown("a")]
            public A TopDown() => null;
        }

        [Language(RuntimeOptions.ForceGenericLR)]
        [ParserGraph(nameof(HighlyAmbiguousLanguage) + ".gv")]
        [DescribeParserStateMachine(nameof(HighlyAmbiguousLanguage) + ".info")]
        public class HighlyAmbiguousLanguage
        {
            [Produce]
            public void Start(E e) { }

            [Produce(null, "+", null)]
            public E Sum(E e1, E e2) => null;

            [Produce("a")]
            public E Ea() => null;
        }

        public interface A {}
        public interface S {}
        public interface E {}
    }
}
