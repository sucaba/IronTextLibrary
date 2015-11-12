using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using IronText.Framework;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace IronText.Runtime
{
    public abstract class LanguageBase : ILanguageRuntime, IInternalInitializable, ILanguageInternalRuntime
    {
        public static class Fields
        {
            public static readonly FieldInfo isDeterministic = ExpressionUtils.GetField((LanguageBase lang) => lang.isDeterministic);

            public static readonly FieldInfo grammar         = ExpressionUtils.GetField((LanguageBase lang) => lang.grammar);

            public static readonly FieldInfo grammarBytes    = ExpressionUtils.GetField((LanguageBase lang) => lang.grammarBytes);

            public static readonly FieldInfo getParserAction = ExpressionUtils.GetField((LanguageBase lang) => lang.getParserAction);

            public static readonly FieldInfo tokenKeyToId    = ExpressionUtils.GetField((LanguageBase lang) => lang.tokenKeyToId);

            public static readonly FieldInfo scan1           = ExpressionUtils.GetField((LanguageBase lang) => lang.scan1);

            public static readonly FieldInfo termFactory     = ExpressionUtils.GetField((LanguageBase lang) => lang.termFactory);

            public static readonly FieldInfo grammarAction   = ExpressionUtils.GetField((LanguageBase lang) => lang.grammarAction);

            public static readonly FieldInfo merge           = ExpressionUtils.GetField((LanguageBase lang) => lang.merge);

            public static readonly FieldInfo name            = ExpressionUtils.GetField((LanguageBase lang) => lang.name);

            public static readonly FieldInfo stateToSymbol   = ExpressionUtils.GetField((LanguageBase lang) => lang.stateToSymbol);

            public static readonly FieldInfo parserConflictActions = ExpressionUtils.GetField((LanguageBase lang) => lang.parserConflictActions);

            public static readonly FieldInfo tokenComplexity = ExpressionUtils.GetField((LanguageBase lang) => lang.tokenComplexity);

            public static readonly FieldInfo matcherToToken  = ExpressionUtils.GetField((LanguageBase lang) => lang.matcherToToken);

            public static readonly FieldInfo createDefaultContext = ExpressionUtils.GetField((LanguageBase lang) => lang.createDefaultContext);
        }

        protected internal bool          isDeterministic;
        protected Grammar                grammar;
        protected byte[]                 grammarBytes;
        protected TransitionDelegate     getParserAction;
        protected Dictionary<object,int> tokenKeyToId;
        protected Scan1Delegate          scan1;
        protected TermFactoryDelegate    termFactory;
        protected ProductionActionDelegate  grammarAction;
        protected MergeDelegate          merge;
        protected CilGrammarSource           name;
        protected int[]                  stateToSymbol;
        protected int[]                  parserConflictActions;
        protected int[]                  tokenComplexity;
        protected int[]                  matcherToToken;
        private ResourceAllocator        allocator;
        protected Func<object>           createDefaultContext;
        private const int maxActionCount = 16;
        private RuntimeGrammar runtimeGrammar;

        public LanguageBase(CilGrammarSource name) 
        { 
            this.name = name;
            this.merge = DefaultMerge;
        }

        public bool IsDeterministic { get { return isDeterministic; } }

        public void Init()
        {
            var formatter = new BinaryFormatter();
            if (grammarBytes != null)
            {
                using (var stream = new MemoryStream(grammarBytes))
                using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress))
                {
                    this.grammar = (Grammar)formatter.Deserialize(decompressStream);
                }

                this.runtimeGrammar = grammar.ToRuntime();
            }

            this.allocator      = new ResourceAllocator(runtimeGrammar);
        }

        public Grammar Grammar { get { return grammar; } }

        public object CreateDefaultContext()
        {
            return createDefaultContext();
        }

        public IScanner CreateScanner(object context, TextReader input, string document, ILogging logging)
        {
            if (logging == null)
            {
                logging = ExceptionLogging.Instance;
            }

            if (context != null)
            {
                ServicesInitializer.SetServiceProperties(
                    context.GetType(),
                    context,
                    typeof(ILanguageRuntime),
                    this);

                ServicesInitializer.SetServiceProperties(
                    context.GetType(),
                    context,
                    typeof(ILogging),
                    logging);
            }

            return new Scanner(scan1, input, document, context, maxActionCount, matcherToToken, logging);
        }

        public IPushParser CreateParser<TNode>(IProducer<TNode> producer, ILogging logging)
        {
            if (isDeterministic)
            {
                return new DeterministicParser<TNode>(
                    producer,
                    runtimeGrammar,
                    getParserAction,
                    allocator,
                    logging
                    );
            }
            else
            {
                return new RnGlrParser<TNode>(
                    runtimeGrammar,
                    tokenComplexity,
                    getParserAction,
                    stateToSymbol,
                    parserConflictActions,
                    producer,
                    allocator,
                    logging);
            }
        }

        public IProducer<ActionNode> CreateActionProducer(object context)
        {
            var result = new ActionProducer(runtimeGrammar, context, grammarAction, termFactory, this.merge, new Dictionary<string,object>());

            return result;
        }

        public int Identify(Type tokenType)
        {
            int result;
            if (!tokenKeyToId.TryGetValue(tokenType, out result))
            {
                result = -1;
            }

            return result;
        }

        public int Identify(string literal)
        {
            int result;
            if (!tokenKeyToId.TryGetValue(literal, out result))
            {
                result = -1;
            }

            return result;
        }

        private object DefaultMerge(
            int                 token,
            object              alt1,
            object              alt2,
            object              context,
            IStackLookback<ActionNode> stackLookback)
        {
            var result = alt2;
#if true
            Debug.WriteLine("------------------------------");
            Debug.WriteLine(
                "Default merging of token {0} values in state {1}:",
                (object)runtimeGrammar.SymbolName(token),
                stackLookback.GetParentState());
            Debug.WriteLine("  '{0}'", alt1);
            Debug.WriteLine(" and");
            Debug.WriteLine("  '{0}'", alt2);
            Debug.WriteLine(" into the value: '{0}'", (object)result);
#endif
            return result;
        }

        RuntimeGrammar ILanguageInternalRuntime.RuntimeGrammar
        {
            get { return runtimeGrammar; }
        }
    }
}
