using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Lib;
using IronText.Lib.Ctem;
using IronText.Lib.Shared;
using NUnit.Framework;
using IronText.Algorithm;
using IronText.Runtime;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class MixedFeaturesTest
    {
        [Test]
        public void StaticMethodTest()
        {
            Assert.AreEqual(555, Eval("555"));

            var spaces = string.Join(
                            "",
                            UnicodeIntSetType.Instance.SpaceSeparator.Select(
                                ord => char.ToString((char)ord)));
            Assert.AreEqual(555, Eval(spaces + "555" + spaces));
            Assert.AreEqual(555, Eval( "/* prefix */ 555 // suffix" + spaces));
        }

        [Test]
        public void Operatives()
        {
            Assert.AreEqual(2, Eval("(- (/ (* (+ 2 5) 5) 7) 3)"));
        }

        [Test]
        public void debugFail()
        {
            Assert.AreEqual(444, Eval("(begin (let x 111) (begin (let x 444) x))"), "Parent frame cell overriding broken");
        }

        [Test]
        public void Scope()
        {
            Assert.AreEqual(444, Eval("(begin (let x 444) x)"), "Single frame definition and use broken");
            Assert.AreEqual(111, Eval("(begin (let x 111) (begin x))"), "Namespace inheritance broken");
            Assert.AreEqual(444, Eval("(begin (let x 111) (begin (let x 444) x))"), "Parent frame cell overriding broken");
            Assert.AreEqual(111, Eval("(begin (let x 111) (begin (let x 444) x) x)"), "Popping child frame broken");

            Assert.Throws<SyntaxException>(() => Eval("x"), "Undefined variable should cause error.");
            Assert.Throws<SyntaxException>(() => Eval("(begin (let y 555) x)"), "Undefined variable should cause error.");
            Assert.Throws<SyntaxException>(() => Eval("(begin x (let x 555) 0)"), "Use before definition should cause error.");
        }

        [Test]
        public void TestEval()
        {
            Assert.AreEqual(11, Eval("(begin (let x 44) (eval \"(/ x 4)\"))"));
        }

        [Test]
        public void ProgrammaticallyPassTokens()
        {
            var language = Language.Get(typeof(MiniInterperter));
            var result = Eval(new[] { OPN, SUM, VAL10, VAL20, VAL10, VAL20, CLS });
            Assert.AreEqual(60, result);

            result = Eval("(sum 10 20 10 20)");
            Assert.AreEqual(60, result);
        }

        private double Eval(string expr)
        {
            var parser = new MiniInterperter();
            return Eval(parser, expr);
        }

        private double Eval(MiniInterperter context, string expr)
        {
            double value = Language.Parse(context, expr).Result;
            return value;
        }

        private double Eval(Msg[] input)
        {
            var parser = new MiniInterperter();
            var result = Language.Parse(parser, input).Result;
            return result;
        }

        private static readonly ILanguageRuntime lang = Language.Get(typeof(MiniInterperter));

        private static readonly Msg OPN   = lang.Literal("(");
        private static readonly Msg CLS   = lang.Literal(")");
        private static readonly Msg VAL0  = lang.Symbol(new Num("0"));
        private static readonly Msg VAL1  = lang.Symbol(new Num("1"));
        private static readonly Msg VAL2  = lang.Symbol(new Num("2"));
        private static readonly Msg VAL3  = lang.Symbol(new Num("3"));
        private static readonly Msg VAL10 = lang.Symbol(new Num("10"));
        private static readonly Msg VAL20 = lang.Symbol(new Num("20"));
        private static readonly Msg DESCR = lang.Symbol(new QStr("long description"));
        private static readonly Msg IF    = lang.Literal("if");
        private static readonly Msg POW   = lang.Literal("^");
        private static readonly Msg SUM   = lang.Literal("sum");

        public interface Locals { }

        [Language]
        [StaticContext(typeof(Builtins))]
        [GrammarDocument("MiniInterpreter.gram")]
        [ScannerDocument("MiniInterpreter.scan")]
        [DescribeParserStateMachine("MiniInterpreter.info")]
        [ScannerGraph("MiniInterpreter.gv")]
        public class MiniInterperter
        {
            public MiniInterperter()
            {
                this.Scope = new DefFirstNs<Locals>(null);
                this.Scanner = new CtemScanner();
            }

            public double Result { get; [Produce] set; }

            [SubContext]
            public CtemScanner Scanner { get; private set; }

            [SubContext]
            public DefFirstNs<Locals> Scope { get; private set; }

            // Static method
            [Produce]
            public static double Num(Num num) { return double.Parse(num.Text); }

            [Produce("(", "let", null, null, ")")]
            public double Let(Def<Locals> def, double rexpr)
            {
                def.Value = rexpr;
                return 0;
            }

            [Produce]
            public double VarRef(Ref<Locals> v) { return (double)v.Value; }

            [Produce("(", "begin", null, null, null, ")")]
            public double Begin(Push<Locals> push, List<double> exprs, Pop<Locals> pop) 
            { 
                return exprs.LastOrDefault();
            }

            [Produce("(", null, null, ")")]
            public double Apply(Ref<Locals> f, List<double> args)
            {
                var lambda = (Lambda<Locals, object>)f.Value;

                Scope.PushFrame();

                try
                {
                    for (int i = lambda.Formals.Count; i != 0; )
                    {
                        --i;
                        Scope.Definition(lambda.Formals[i]).Value = args[i];
                    }

                    return Language.Parse(this, lambda.Body).Result;
                }
                finally
                {
                    Scope.PopFrame();
                }
            }

            [Produce("(", "eval", null, ")")]
            public double Eval(QStr str) 
            { 
                return Language.Parse(this, str.Text).Result; 
            }

            [Produce("(", "+", null, null, ")")]
            public double Plus(double x, double y) { return x + y; }

            [Produce("(", "-", null, null, ")")]
            public double Minus(double x, double y) { return x - y; }

            [Produce("(", "*", null, null, ")")]
            public double Multiply(double x, double y) { return x * y; }

            [Produce("(", "/", null, null, ")")]
            public double Divide(double x, double y) { return x / y; }

            [Produce("(", "^", null, null, ")")]
            public double Pow(double x, double y) { return Math.Pow(x, y); }

            [Produce(null, "^", null)]
            public double InfixPow(double x, Num y) { return Math.Pow(x, Num(y)); }

            [Produce("(", "sum", null, ")")]
            public double Sum(List<double> args) { return args.Sum(); }


            class Lambda<TNs, T>
            {
                public IFrame<TNs> ClosedFrame;
                public List<string> Formals;
                public Msg[] Body;
            }
        }
    }
}
