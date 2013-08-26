using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IronText.Lib.Sre;
using IronText.Tests.TestUtils;

namespace IronText.Tests.Lib.NfaVM
{
    class SreBenchmark
    {
        //private const string Pattern = @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b";
        private const string Pattern = @"x[ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab][ab]y";
        private const string SrePattern = @"(: #\x (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) (or #\a #\b) #\y)";
        private const string TestInput = "xabababababababababababababababababababababababababy";
        private const int CompilationSampleCount = 1;
        private const int ExecutionSampleCount = 10000;
        
        public long CompileTimeTicks;
        public long ExecPreparationTimeTicks;
        public long ExecutionTimeTicks;
        private static bool HasNativeTiming;
        public static long NativeCompileTimeTicks;
        public static long NativeExecutionTimeTicks;
        private string name;

        public SreBenchmark(string name)
        {
            this.name = name;
        }

        public void RunBenchmark(Func<string,SRegex> compile)
        {
            Benchmark(compile);
            if (!HasNativeTiming)
            {
                HasNativeTiming = true;
                NativeBenchmark();
            }
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            output
                .AppendFormat("{0, 20}: compile time = {1} ({2:P})", name, CompileTimeTicks, ((double)CompileTimeTicks) / NativeCompileTimeTicks)
                .AppendLine()
                .AppendFormat("{0, 20}: execute time = {1} ({2:P})", name, ExecutionTimeTicks, ((double)ExecutionTimeTicks) / NativeExecutionTimeTicks)
                .AppendLine()
                .AppendFormat("{0, 20}: execute prep. time = {1} ({2:P})", name, ExecPreparationTimeTicks, ((double)ExecPreparationTimeTicks) / NativeExecutionTimeTicks)
                .AppendLine()
                .AppendFormat("{0, 20}: native compile time = {1}", name, NativeCompileTimeTicks)
                .AppendLine()
                .AppendFormat("{0, 20}: native execute time = {1}", name, NativeExecutionTimeTicks)
                .AppendLine();
            return output.ToString();
        }

        private void Benchmark(Func<string, SRegex> compile)
        {
            SRegex sre = null;
            bool result = false;
            CompileTimeTicks = Bench.Measure(() => { sre = compile(SrePattern); }, CompilationSampleCount);
            ExecutionTimeTicks = Bench.Measure(() => result = sre.Match(TestInput), ExecutionSampleCount);
            ExecPreparationTimeTicks = Bench.Measure(() => TestInput.Select(ch => (int)ch).ToArray(), ExecutionSampleCount);
            if (!result)
            {
                throw new InvalidOperationException("Internal test error");
            }
        }

        private static void NativeBenchmark()
        {
            Regex re = null;
            bool result = false;
            NativeCompileTimeTicks = Bench.Measure(() => re = new Regex(Pattern, RegexOptions.Compiled), CompilationSampleCount);
            NativeExecutionTimeTicks = Bench.Measure(() => result = re.Match(TestInput).Success, ExecutionSampleCount);
            if (!result)
            {
                throw new InvalidOperationException("Internal test error");
            }
        }
    }
}
