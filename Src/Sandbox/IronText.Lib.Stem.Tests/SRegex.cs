using System.Linq;
using IronText.Automata.Regular;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.NfaVM.Backend.BytecodeCompiler;
using IronText.Lib.NfaVM.ILCompiler;
using IronText.Lib.NfaVM.Runtime;
using IronText.Lib.RegularAst;
using IronText.Lib.RegularAst.Backend.InMemoryNfaCompiler;
using IronText.Lib.Shared;
using IronText.Tests.Algorithm;

namespace IronText.Lib.Sre
{
    public delegate bool MatchDelegate(int[] input);

    public enum SRegexOptions
    {
        ByteCodeCompilation,
        ILCompilation,
        NfaCompilation,
        DfaCompilation,
        NativeRegex,
        Default = ByteCodeCompilation,
    }

    public class SRegex
    {
        private static int PatternID = 0;
        private MatchDelegate matcher;

        public SRegex(string pattern, SRegexOptions options = SRegexOptions.Default)
        {
            var ast = Language.Parse(new SreSyntax(), pattern).Result.Node;
            switch (options)
            {
                case SRegexOptions.ByteCodeCompilation:
                    var compiler = new NfaVMBytecodeBackend(ast);
                    matcher = (int[] input) =>
                        {
                            var vm = new PikeNfaVM(compiler.Code.ToArray());
                            vm.Feed(input.Select(ch => (int)ch)).Done();
                            return vm.HasMatch;
                        };
                    break;
                case SRegexOptions.ILCompilation:
                    string methodName = "MatchSrePattern" + PatternID++;

                    var builder = new CachedMethod<MatchDelegate>(
                        methodName,
                        (emit, args) => EmitAst(emit, ast, args[0]));

                    matcher = builder.Delegate;
                    break;
                case SRegexOptions.NfaCompilation:
                    var nfa = new Nfa(ast);
                    matcher = nfa.Match;
                    break;
                case SRegexOptions.DfaCompilation:
                    var dfa = new RegularToDfaAlgorithm(new RegularTree(ast));
                    var simulation = new DfaSimulation(dfa.Data);
                    matcher = input => simulation.Match(input);
                    break;
            }
        }

        public bool Match(string input)
        {
            int len = input.Length;
            var arr = new int[len];
            while(len != 0)
            {
                --len;
                arr[len] = input[len];
            }

            return matcher(arr);
        }

        private static EmitSyntax EmitAst(EmitSyntax emit, AstNode ast, Ref<Args> input)
        {
            var settings = new ILCompilerSettings
            { 
                LdInput = il => il.Ldarg(input),
                SUCCESS = emit.Labels.Generate(),
                FAILURE = emit.Labels.Generate(),
            };

            var _ = new ILNfaCompiler(ast, emit, settings);

            emit
                .Label(settings.SUCCESS)
                .Ldc_I4_1()
                .Ret()
                .Label(settings.FAILURE)
                .Ldc_I4_0()
                .Ret();
                
            return emit;
        }
    }
}
