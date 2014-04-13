using System;
using System.IO;
using IronText.Lib.IL;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;
using System.Reflection.Emit;
using System.Reflection;

namespace IronText.MetadataCompiler
{
    class BootstrapLanguage : ILanguageRuntime, IInternalInitializable, IBootstrapLanguage, ILanguageInternalRuntime
    {
        private delegate object TokenFactoryDelegate1(string text, object rootContext);

        private readonly LanguageData data;
        private readonly CilGrammarSource source;
        private readonly ProductionActionDelegate grammarAction;
        private readonly MergeDelegate merge;
        private ResourceAllocator allocator;
        private RuntimeGrammar runtimeGrammar;
        private TokenFactoryDelegate1[] tokenFactories;
        private readonly ScannerDescriptor scannerDescriptor;

        public BootstrapLanguage(CilGrammarSource source, LanguageData data)
        {
            this.source = source;
            this.data = data;
            this.grammarAction = BuildExecuteRuleAction();
            this.merge = (int token, object x, object y, object context, IStackLookback<Msg> stackLookback) => y;

            this.scannerDescriptor = ScannerDescriptor.FromScanRules(data.Grammar.Matchers, ExceptionLogging.Instance);
            int count = scannerDescriptor.Matchers.Count;

            this.tokenFactories = new TokenFactoryDelegate1[count];
            for (int i = 0; i != count; ++i)
            {
                if (scannerDescriptor.Matchers[i].Outcome != null)
                {
                    tokenFactories[i] = BuildTokenFactory(scannerDescriptor.Matchers[i]);
                }
            }
        }

        public void Init()
        {
            this.runtimeGrammar = new RuntimeGrammar(data.Grammar);
            this.allocator = new ResourceAllocator(runtimeGrammar);
        }

        public bool IsDeterministic { get { return data.IsDeterministic; } }

        public Grammar Grammar { get { return data.Grammar; } }

        public object CreateDefaultContext()
        {
            return Activator.CreateInstance(source.DefinitionType);
        }

        public IScanner CreateScanner(object context, TextReader input, string document, ILogging logging)
        {
            return new BootstrapScanner(
                input,
                document,
                scannerDescriptor,
                context,
                logging);
        }

        public IPushParser CreateParser<TNode>(IProducer<TNode> producer, ILogging logging)
        {
            return new DeterministicParser<TNode>(
                producer,
                runtimeGrammar,
                GetParserAction,
                allocator,
                logging
                );
        }

        public IProducer<Msg> CreateActionProducer(object context)
        {
            return new ActionProducer(runtimeGrammar, context, grammarAction, this.TermFactory,  this.merge);
        }

        private int GetParserAction(int state, int token)
        {
            return data.ParserActionTable.Get(state, token);
        }

        public int Identify(Type type)
        {
            throw new NotSupportedException();
        }

        public int Identify(string literal)
        {
            throw new NotSupportedException();
        }

        private ProductionActionDelegate BuildExecuteRuleAction()
        {
            var generator = new ProductionActionGenerator();
            var result = new CachedMethod<ProductionActionDelegate>(
                "Bootstrap." + source.FullLanguageName + ".GrammarActions",
                (emit, args) => 
                { 
                    generator.BuildBody(emit, data, args);
                    return emit.Ret();
                }).Delegate;
            return result;
        }

        RuntimeGrammar ILanguageInternalRuntime.RuntimeGrammar
        {
            get { return runtimeGrammar; }
        }

        private static TokenFactoryDelegate1 BuildTokenFactory(Matcher scanProduction)
        {
            var method = new DynamicMethod(
                                "Create",
                                typeof(object), 
                                new[] { typeof(string), typeof(object) },
                                typeof(BootstrapScanner).Module);

            var il = method.GetILGenerator(256);

            if (scanProduction.Pattern.IsLiteral)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                var binding = scanProduction.Joint.The<CilMatcher>();
                if (binding == null)
                {
                    throw new InvalidOperationException("ScanProduction is missing CIL platform binding.");
                }

                Type type = binding.BootstrapResultType;

                MethodInfo parseMethod = GetStaticParseMethod(type);
                if (parseMethod != null)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, parseMethod);
                }
                else if (type == typeof(string))
                {
                    il.Emit(OpCodes.Ldarg_0);
                }
                else
                {
                    ConstructorInfo constructor = null;
                    Type[] paramTypes = SelectConstructor(type, ref constructor);
                    if (paramTypes.Length > 0)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        if (paramTypes.Length > 1)
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                    }

                    il.Emit(OpCodes.Newobj, constructor);
                }
            }

            il.Emit(OpCodes.Ret);
            return (TokenFactoryDelegate1)method.CreateDelegate(typeof(TokenFactoryDelegate1));
        }

        private static Type[] SelectConstructor(Type type, ref ConstructorInfo constructor)
        {
            Type[][] signatures = {
                    new Type[] { typeof(string) },
                    Type.EmptyTypes
                };


            Type[] paramTypes = null;
            foreach (var signature in signatures)
            {
                constructor = type.GetConstructor(signature);
                if (constructor != null)
                {
                    paramTypes = signature;
                    break;
                }
            }

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    "Type " + type.FullName + " has no public constructor suitable for a lexer.");
            }
            return paramTypes;
        }

        private static MethodInfo GetStaticParseMethod(Type type)
        {
            if (type == null)
            {
                return null;
            }

            var m = type.GetMethod(
                        "BootstrapParse",
                        BindingFlags.Static | BindingFlags.Public,
                        null, new[] { typeof(string) },
                        null)
                     ??
                     type.GetMethod(
                        "Parse",
                        BindingFlags.Static | BindingFlags.Public,
                        null, new[] { typeof(string) },
                        null)
                     ;
            return m;
        }

        private object TermFactory(object context, int action, string text)
        {
            return tokenFactories[action](text, context);
        }
    }
}
