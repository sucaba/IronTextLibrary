using IronText.Logging;
using System;
using System.IO;

namespace IronText.Runtime
{
    public interface ILanguageRuntime
    {
        RuntimeGrammar RuntimeGrammar { get; }

        /// <summary>
        /// Determines whether parsing algorithm being used is deterministic.
        /// </summary>
        bool IsDeterministic { get; }

        object CreateDefaultContext();

        IProducer<ActionNode> CreateProducer(object context);

        IScanner CreateScanner(object context, TextReader input, string document, ILogging logging = null);

        IPushParser CreateParser<TNode>(IProducer<TNode> producer, ILogging logging = null);

        int Identify(string literal);

        int Identify(Type symbolType);
    }

    public static class LanguageExtensions
    {
        private const string UnknownText = "?";

        public static Msg Literal(this ILanguageRuntime @this, string literal)
        {
            var id = @this.Identify(literal);
            return new Msg(id, literal, null, Loc.Unknown);
        }

        public static Msg Symbol<T>(this ILanguageRuntime @this, T value, string text = UnknownText)
        {
            return @this.Symbol(typeof(T), value, text);
        }

        public static Msg Symbol(this ILanguageRuntime @this, Type type, object value, string text = UnknownText)
        {
            return new Msg(@this.Identify(type), text, value, Loc.Unknown);
        }

        private static int IdentifySymbolValue(this ILanguageRuntime @this, object value)
        {
            return @this.Identify(value.GetType());
        }
    }
}
