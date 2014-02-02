using System.Diagnostics;
using System.IO;
using IronText.Diagnostics;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Logging;
using IronText.Runtime;
using IronText.Tests.Data;
using IronText.Tests.TestUtils;
using NUnit.Framework;

namespace IronText.Stem.Tests.Lib.IL
{
    [TestFixture]
    public class ParserTest
    {
        [Test]
        public void HeatupTest()
        {
            var filePath = DataSamples.CompileSample1FilePath;
            var source = new StreamReader(filePath);
            ILanguageRuntime lang;
            
            // Get language
            {
                var timer0 = new Stopwatch();
                timer0.Start();
                lang = Language.Get(typeof(ILLanguage));
                timer0.Stop();
                Trace.WriteLine("get lang:  " + timer0.Elapsed);
            }

            // Cold state: Slow because of loading big methods:
            {
                var timer0 = new Stopwatch();
                using (var interp = new Interpreter(lang))
                {
                    interp.LogKind = LoggingKind.None;
                    timer0.Start();
                    interp.Parse("");
                    timer0.Stop();
                }

                Trace.WriteLine("heatup: " + timer0.Elapsed);
            }

            // Hot state: should be fast 
            {
                var timer0 = new Stopwatch();
                using (var interp = new Interpreter(lang))
                {
                    interp.LogKind = LoggingKind.None;
                    timer0.Start();
                    interp.Parse("");
                    timer0.Stop();
                }

                Trace.WriteLine("heatup: " + timer0.Elapsed);
            }
        }

        [Test]
        public void Test0()
        {
            var filePath = DataSamples.CompileSample0FilePath;
            string output = BuildAndRun(filePath);
            StringAssert.Contains("I'm from the IL Assembly Language...", output);
        }

        [Test]
        public void Test1()
        {
            var filePath = DataSamples.CompileSample1FilePath;
            string output = BuildAndRun(filePath);
            StringAssert.Contains("I'm from the IL Assembly Language...", output);
        }

        [Test]
        public void Test2()
        {
            var filePath = DataSamples.CompileSample2FilePath;
            string output = BuildAndRun(filePath);
            StringAssert.Contains("3628800", output);
        }

        [Test]
        public void Test2Sppf()
        {
            var filePath = DataSamples.CompileSample2FilePath;

            using (var source = new StreamReader(filePath))
            using (var interpreter = new Interpreter<ILLanguage>())
            {
                SppfNode sppf = interpreter.BuildTree(source, filePath);

                using (var graph = new GvGraphView("Cil_Sample2_sppf.gv"))
                {
                    var lang = Language.Get(typeof(ILLanguage));
                    sppf.WriteGraph(graph, lang.Grammar, true);
                }
            }
        }

        [Test]
        public void Test3()
        {
            var filePath = DataSamples.CompileSample3FilePath;
            string output = BuildAndRun(filePath);
            StringAssert.Contains("I'm from the IL Assembly Language...", output);
        }

        [Test]
        public void Test4()
        {
            var filePath = DataSamples.CompileSample4FilePath;
            string output = BuildAndRun(filePath);
            StringAssert.Contains("success!", output);
        }

        private static string BuildAndRun(string filePath)
        {
            ILLanguage backend = new CecilBackendLanguage(filePath);

            using (var interpreter = new Interpreter<ILLanguage>(backend))
            using (var source = new StreamReader(filePath))
            {
                if (!interpreter.Parse(source, filePath))
                {
                    throw new AssertionException("Parsing of file " + filePath + " failed!");
                }
            }

            backend.Save();

            return ProgramExecutor.Execute(Path.GetFileNameWithoutExtension(filePath) + ".exe");
        }
    }

    [Language(LanguageFlags.AllowNonDeterministic)]
    [GrammarDocument("Cil.gram")]
    [ScannerDocument("Cil.scan")]
    [ParserGraph("Cil_Parser.gv")]
    [ScannerGraph("Cil_Scanner.gv")]
    [DescribeParserStateMachine("Cil.info")]
    public interface ILLanguage
    {
        [SubContext]
        CilSyntax Syntax { get; }

        void Save();
    }

    class CecilBackendLanguage : ILLanguage
    {
        private string outputPath;

        public CecilBackendLanguage(string filePath)
        {
            Syntax = CilBackend.CreateCompiler(filePath);
            outputPath = Path.GetFileNameWithoutExtension(filePath) + ".exe";
        }

        public CilSyntax Syntax { get; private set; }

        public void Save()
        {
            var writer = Syntax as IAssemblyWriter;
            writer.Write(outputPath);
        }
    }

}
