using System;
using System.Collections.Generic;
using IronText.Framework;
using IronText.Lib;
using IronText.Logging;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ErrorRecoveryTest
    {
        [Datapoints]
        public static Type[] LanguageDefs
        {
            get
            {
                return new[]
                    {
                        typeof(RecoveryLang),
                        typeof(AmbRecoveryLang),
                    };
            }
        }

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
        public void PositiveParseTest(Type langDef)
        {
            var lang = Language.Get(langDef);

            Parse(lang, "{ callFunc(); { callFunc(); callFunc () ; } callFunc(); }");
            AssertErrors(0, 0);
        }

        [Theory]
        public void ScannerErrorRecovery(Type langDef)
        {
            var lang = Language.Get(langDef);

            Parse(lang, "{ $\n $ $callFunc(); }");
            AssertErrors(0, 3);

            Reset();

            Parse(lang, "ac();");
            AssertErrors(0, 3); // 2 scanner errors on 'a' and on '(' then 1 panic-mode error.
        }

        [Theory]
        public void TailScannerErrorRecovery(Type langDef)
        {
            var lang = Language.Get(langDef);

            Parse(lang, "callF");
            AssertErrors(0, 2, "One lexical error for incomplete token and one syntax error for unexpected EOI"); 
        }

        [Theory]
        public void LocalCorrections(Type langDef)
        {
            var lang = Language.Get(langDef);

            // 3 local error corrections: insert, replace, delete
            Parse(lang, "{ callFunc); \n callFunc((; \n callFunc() \n  callFunc; \n } ");
            AssertErrors(0, 3);
        }

        [Theory]
        public void LocalCorrectionWithBeacon(Type langDef)
        {
            var lang = Language.Get(langDef);
            
            Parse(lang, "{ callFunc() beacon callFunc } ");
            AssertErrors(0, 2);
        }

        [Theory]
        public void TailLocalCorrections(Type langDef)
        {
            var lang = Language.Get(langDef);

            Parse(lang, "{ callFunc();");
            AssertErrors(0, 1);

            Reset();

            Parse(lang, "callFunc)");
            AssertErrors(0, 2);
        }

        [Theory]
        public void ErrorProductionRecovery(Type langDef)
        {
            var lang = Language.Get(langDef);

            Parse(lang,  "{ callFunc callFunc callFunc callFunc callFunc callFunc } ");
            AssertErrors(1, 0);

            Reset();

            Parse(lang, "{ callFunc callFunc callFunc callFunc { callFunc } ");
            AssertErrors(1, 0);
        }

        [Theory]
        public void PanicModeRecovery(Type langDef)
        {
            var lang = Language.Get(langDef);

            Parse(lang, "callFunc callFunc callFunc callFunc callFunc beacon");
            AssertErrors(0, 1);
        }

        [Theory]
        public void AllLevelsRecovery(Type langDef)
        {
            var lang = Language.Get(langDef);

            var context = new RecoveryLang();

            Assert.IsTrue(lang.Grammar.Symbols[lang.Identify("beacon")].Categories.Has(SymbolCategory.Beacon));
            Assert.IsTrue(lang.Grammar.Symbols[lang.Identify("{")].Categories.Has(SymbolCategory.DoNotInsert));
            Assert.IsTrue(lang.Grammar.Symbols[lang.Identify("{")].Categories.Has(SymbolCategory.DoNotDelete));

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

        private void AssertErrors(int expectedProductions, int expectedErrors, string message = null)
        {
            Assert.AreEqual(expectedErrors + expectedProductions, errorCount, message);
        }

        [Language]
        [DescribeParserStateMachine("RecoveryLang.info")]
        [StaticContext(typeof(Builtins))]
        [ParserGraph("RecoveryLang_Parser.gv")]
        [ScannerGraph("RecoveryLang_Scanner.gv")]
        [TokenCategory("beacon", SymbolCategory.Beacon|SymbolCategory.DoNotInsert)]
        [TokenCategory("{", SymbolCategory.DoNotInsert|SymbolCategory.DoNotDelete)]
        public class RecoveryLang
        {
            [Produce]
            public void Start(Stmt stmt) { }

            [Produce("{", null, "}")]
            public Stmt Block(List<Stmt> stmts) { return null; }

            [Produce("beacon")]
            public Stmt BeaconStmt() { return null; }

            // Method body is never executed because action
            // producer was replaced with null-producer after
            // first error is detected. However method signature
            // is used for error-production recovery.
            [Produce("{", null, "}")]
            public Stmt BlockError(Exception error) { return null; }

            [Produce("callFunc", "(", ")", ";")]
            public Stmt CallFunc() { return null; }

            [Match("([\n\r] | blank)+")]
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
