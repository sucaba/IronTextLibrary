using System;
using System.IO;
using IronText.Lib.IL;
using IronText.Logging;
using IronText.Runtime;
using System.Collections.Generic;

namespace IronText.MetadataCompiler
{
    class BootstrapLanguage
        : ILanguageRuntime
        , IInternalInitializable
        , IBootstrapLanguage
        , ISourceGrammarProvider
    {
        private delegate object TokenFactoryDelegate1(string text, object rootContext);

        private readonly LanguageData data;
        private readonly TypedLanguageSource source;
        private readonly ProductionActionDelegate grammarAction;
        private readonly TermFactoryDelegate termFactory;
        private readonly ShiftActionDelegate shiftAction;
        private readonly MergeDelegate merge;
        private readonly ScannerDescriptor scannerDescriptor;

        public BootstrapLanguage(TypedLanguageSource source, LanguageData data)
        {
            this.source        = source;
            this.data          = data;
            this.grammarAction = BuildExecuteRuleAction();
            this.termFactory   = BuildTermFactory();
            this.shiftAction   = null;
            this.merge = (
                        int token,
                        object x,
                        object y,
                        object context,
                        IStackLookback<ActionNode> stackLookback
                        ) => y;

            this.scannerDescriptor = ScannerDescriptor.FromScanRules(data.Grammar.Matchers, ExceptionLogging.Instance);
        }

        public RuntimeGrammar RuntimeGrammar { get; private set; }

        public void Init()
        {
            this.RuntimeGrammar = data.RuntimeGrammar;
        }

        public ParserRuntime TargetParserRuntime => data.TargetParserRuntime;

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
            return new DeterministicParser<TNode>(producer, RuntimeGrammar, GetParserAction, logging);
        }

        public IProducer<ActionNode> CreateProducer(object context)
        {
            return new ActionProducer(
                        RuntimeGrammar,
                        context,
                        grammarAction,
                        termFactory,
                        shiftAction,
                        this.merge,
                        new Dictionary<int,object>());
        }

        private int GetParserAction(int state, int token)
        {
            int start = data.ParserActionStartTable.Get(state, token);
            return start;
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
                },
                saveFile: true).Delegate;
            return result;
        }

        private TermFactoryDelegate BuildTermFactory()
        {
            var generator = new TermFactoryGenerator(data);
            var result = new CachedMethod<TermFactoryDelegate>(
                "Bootstrap." + source.FullLanguageName + ".TermFactory",
                (emit, args) => 
                { 
                    return generator.Build(emit, args);
                }).Delegate;
            return result;
        }

        object ISourceGrammarProvider.GetSourceGrammar()
        {
            return this.data.Grammar;
        }
    }
}
