using System;
using System.IO;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.IL;
using IronText.Runtime;
using IronText.Logging;
using IronText.Reflection.Managed;

namespace IronText.MetadataCompiler
{
    class BootstrapLanguage : ILanguageRuntime, IInternalInitializable, IBootstrapLanguage, ILanguageInternalRuntime
    {
        private readonly LanguageData data;
        private readonly CilGrammarSource source;
        private readonly ProductionActionDelegate grammarAction;
        private readonly MergeDelegate merge;
        private ResourceAllocator allocator;
        private RuntimeGrammar runtimeGrammar;

        public BootstrapLanguage(CilGrammarSource source, LanguageData data)
        {
            this.source = source;
            this.data = data;
            this.grammarAction = BuildExecuteRuleAction();
            this.merge = (int token, object x, object y, object context, IStackLookback<Msg> stackLookback) => y;
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
                ScannerDescriptor.FromScanRules(data.Grammar.Conditions[0].Matchers, ExceptionLogging.Instance),
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
            return new ActionProducer(runtimeGrammar, context, grammarAction, this.merge);
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
            var generator = new GrammarActionGenerator();
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
    }
}
