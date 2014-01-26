using System;
using System.IO;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.IL;

namespace IronText.MetadataCompiler
{
    class BootstrapLanguage : ILanguage, IInternalInitializable, IBootstrapLanguage, ILanguageRuntime
    {
        private readonly LanguageData data;
        private readonly LanguageName name;
        private readonly ProductionActionDelegate grammarAction;
        private readonly MergeDelegate merge;
        private ResourceAllocator allocator;
        private RuntimeEbnfGrammar runtimeGrammar;

        public BootstrapLanguage(LanguageName name, LanguageData data)
        {
            this.name = name;
            this.data = data;
            this.grammarAction = BuildExecuteRuleAction();
            this.merge = (int token, object x, object y, object context, IStackLookback<Msg> stackLookback) => y;
        }

        public void Init()
        {
            this.runtimeGrammar = new RuntimeEbnfGrammar(data.Grammar);
            this.allocator = new ResourceAllocator(runtimeGrammar);
        }

        public bool IsDeterministic { get { return data.IsDeterministic; } }

        public LanguageName Name { get { return name; } }

        public EbnfGrammar Grammar { get { return data.Grammar; } }

        public void Heatup() { }

        public object CreateDefaultContext()
        {
            return Activator.CreateInstance(name.DefinitionType);
        }

        public IScanner CreateScanner(object context, TextReader input, string document, ILogging logging)
        {
            return new BootstrapScanner(
                input,
                document,
                ScannerDescriptor.FromScanRules(
                    name.LanguageTypeName + "_Lexer",
                    data.Grammar.ScanConditions[0].ScanProductions,
                    ExceptionLogging.Instance),
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
                "Bootstrap." + name.FullName + ".GrammarActions",
                (emit, args) => 
                { 
                    generator.BuildBody(emit, data, args);
                    return emit.Ret();
                }).Delegate;
            return result;
        }

        RuntimeEbnfGrammar ILanguageRuntime.RuntimeGrammar
        {
            get { return runtimeGrammar; }
        }
    }
}
