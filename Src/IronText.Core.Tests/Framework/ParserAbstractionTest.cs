using IronText.Framework;
using IronText.Lib.Ctem;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ParserAbstractionTest
    {
        [Test]
        public void BaseAbstractClassTest()
        {
            var context = new CalcExprInterpModule();
            var result = (CalcExprInterp)Language.Parse<CalcExprBaseModule>(context, "(+ 1 (+ 2 3))").Result;
            Assert.AreEqual(6, result.Value);
        }

        [Test]
        public void BaseInterfaceTest()
        {
            var context = new CalcExprInterpModule2();
            var result = (CalcExprInterp2)Language.Parse<ICalcExprBaseModule2>(context, "(+ 1 (+ 2 3))").Result;
            Assert.AreEqual(6, result.Value);
        }

        public abstract class CalcExprBase { }

        public class CalcExprInterp : CalcExprBase { public double Value; }

        [Language]
        public abstract class CalcExprBaseModule
        {
            public CalcExprBase Result { get; [Parse] set; }

            [SubContext]
            public abstract CtemScanner Scanner { get; }

            [Parse]
            public abstract CalcExprBase Number(Num num);

            [Parse("(", "+", null, null, ")")]
            public abstract CalcExprBase Plus(CalcExprBase x, CalcExprBase y);

            [Parse("(", "-", null, null, ")")]
            public abstract CalcExprBase Minus(CalcExprBase x, CalcExprBase y);
        }

        public class CalcExprInterpModule : CalcExprBaseModule
        {
            private readonly CtemScanner _scanner;

            public CalcExprInterpModule()
            {
                _scanner = new CtemScanner();
            }

            public override CtemScanner Scanner { get { return _scanner; } }

            public override CalcExprBase Number(Num num) { return new CalcExprInterp { Value = double.Parse(num.Text) }; }

            public override CalcExprBase Plus(CalcExprBase x, CalcExprBase y) { return new CalcExprInterp { Value = Val(x) + Val(y) }; }

            public override CalcExprBase Minus(CalcExprBase x, CalcExprBase y) { return new CalcExprInterp { Value = Val(x) - Val(y) }; }

            static double Val(CalcExprBase x) { return ((CalcExprInterp)x).Value; }
        }

        //  Same but with interfaces instead of abstract classes

        public interface ICalcExprBase2 { }

        public class CalcExprInterp2 : ICalcExprBase2 { public double Value; }

        [Language]
        public interface ICalcExprBaseModule2
        {
            ICalcExprBase2 Result { get; [Parse] set; }

            [SubContext]
            CtemScanner Scanner { get; }

            [Parse]
            ICalcExprBase2 Number(Num num);

            [Parse("(", "+", null, null, ")")]
            ICalcExprBase2 Plus(ICalcExprBase2 x, ICalcExprBase2 y);

            [Parse("(", "-", null, null, ")")]
            ICalcExprBase2 Minus(ICalcExprBase2 x, ICalcExprBase2 y);
        }

        public class CalcExprInterpModule2 : ICalcExprBaseModule2
        {
            public CalcExprInterpModule2()
            {
                Scanner = new CtemScanner();
            }

            public ICalcExprBase2 Result { get; set; }

            public CtemScanner Scanner { get; private set; }

            public ICalcExprBase2 Number(Num num) { return new CalcExprInterp2 { Value = double.Parse(num.Text) }; }

            public ICalcExprBase2 Plus(ICalcExprBase2 x, ICalcExprBase2 y) { return new CalcExprInterp2 { Value = Val(x) + Val(y) }; }

            public ICalcExprBase2 Minus(ICalcExprBase2 x, ICalcExprBase2 y) { return new CalcExprInterp2 { Value = Val(x) - Val(y) }; }

            static double Val(ICalcExprBase2 x) { return ((CalcExprInterp2)x).Value; }
        }
    }
}
