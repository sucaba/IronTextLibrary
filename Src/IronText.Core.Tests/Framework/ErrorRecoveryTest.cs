using System;
using System.Collections.Generic;
using IronText.Framework;
using IronText.Lib;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ErrorRecoveryTest
    {
        [Datapoints]
        public static readonly ILanguage[] Languages = 
        {
            Language.Get(typeof(RecoveryLang)),
            Language.Get(typeof(AmbRecoveryLang)),
        };

        private RecoveryLang context;
        private int errorCount;

        private void Reset()
        {
            context = new RecoveryLang();
            errorCount = 0;
        }

        [SetUp]
        public void SetUp()
        {
            Reset();
        }

        [Theory]
        public void PositiveParseTest(ILanguage lang)
        {
            Parse(lang, "{ callFunc(); { callFunc(); callFunc () ; } callFunc(); }");
            AssertErrors(0, 0);
        }

        [Theory]
        public void ScannerErrorRecovery(ILanguage lang)
        {
            Parse(lang, "{ $\n $ $callFunc(); }");
            AssertErrors(0, 3);

            Reset();

            Parse(lang, "ac();");
            AssertErrors(0, 3); // 2 scanner errors on 'a' and on '(' then 1 panic-mode error.
        }

        [Theory]
        public void TailScannerErrorRecovery(ILanguage lang)
        {
            Parse(lang, "callF");
            AssertErrors(0, 1);
        }

        [Theory]
        public void LocalCorrections(ILanguage lang)
        {
            // 3 local error corrections: insert, replace, delete
            Parse(lang, "{ callFunc); \n callFunc((; \n callFunc() \n  callFunc; \n } ");
            AssertErrors(0, 3);
        }

        [Theory]
        public void LocalCorrectionWithBeacon(ILanguage lang)
        {
            Parse(lang, "{ callFunc() beacon callFunc } ");
            AssertErrors(0, 2);
        }

        [Theory]
        public void TailLocalCorrections(ILanguage lang)
        {
            Parse(lang, "{ callFunc();");
            AssertErrors(0, 1);

            Reset();

            Parse(lang, "callFunc)");
            AssertErrors(0, 2);
        }

        [Theory]
        public void ErrorProductionRecovery(ILanguage lang)
        {
            Parse(lang,  "{ callFunc callFunc callFunc callFunc callFunc callFunc } ");
            AssertErrors(1, 0);

            Reset();

            Parse(lang, "{ callFunc callFunc callFunc callFunc { callFunc } ");
            AssertErrors(1, 0);
        }

        [Theory]
        public void PanicModeRecovery(ILanguage lang)
        {
            Parse(lang, "callFunc callFunc callFunc callFunc callFunc beacon");
            AssertErrors(0, 1);
        }

        [Theory]
        public void AllLevelsRecovery(ILanguage lang)
        {
            var context = new RecoveryLang();

            Assert.IsTrue(lang.Grammar.IsBeacon(lang.Identify("beacon")));
            Assert.IsTrue(lang.Grammar.IsDontInsert(lang.Identify("{")));
            Assert.IsTrue(lang.Grammar.IsDontDelete(lang.Identify("{")));

            Parse(
                lang, 
                // 3 errors:
                //  1st handled by panic mode
                //  2nd uses error-production rule. 
                //  3rd reports missing '}'
                "{ callFunc callFunc \n callFunc callFunc \n callFunc beacon { callFunc \n callFunc \n callFunc } \n callFunc(); \n");

            AssertErrors(1, 2);
        }

        private void Parse(ILanguage lang, string input)
        {
            using (var interp = new Interpreter(context, lang))
            {
                interp.LogKind = LoggingKind.Collection;
                interp.Parse(input);
                errorCount = interp.ErrorCount;
            }
        }

        private void AssertErrors(int expectedProductions, int expectedErrors)
        {
            Assert.AreEqual(expectedErrors + expectedProductions, errorCount);
        }

        [Language]
        [DescribeParserStateMachine("RecoveryLang.info")]
        [StaticContext(typeof(Builtins))]
        [ParserGraph("RecoveryLang_Parser.gv")]
        [ScannerGraph("RecoveryLang_Scanner.gv")]
        [TokenCategory("beacon", TokenCategory.Beacon|TokenCategory.DoNotInsert)]
        [TokenCategory("{", TokenCategory.DoNotInsert|TokenCategory.DoNotDelete)]
        public class RecoveryLang
        {
            [Parse]
            public void Start(Stmt stmt) { }

            [Parse("{", null, "}")]
            public Stmt Block(List<Stmt> stmts) { return null; }

            [Parse("beacon")]
            public Stmt BeaconStmt() { return null; }

            // Method body is never executed because action
            // producer was replaced with null-producer after
            // first error is detected. However method signature
            // is used for error-production recovery.
            [Parse("{", null, "}")]
            public Stmt BlockError(Exception error) { return null; }

            [Parse("callFunc", "(", ")", ";")]
            public Stmt CallFunc() { return null; }

            [Scan("([\n\r] | blank)+")]
            public void Blank() { }
        }

        [Language(LanguageFlags.ForceNonDeterministic)]
        [ParserGraph("AmbRecoveryLang.gv")]
        public class AmbRecoveryLang : RecoveryLang
        {
        }

        public interface Stmt { }
    }
}
