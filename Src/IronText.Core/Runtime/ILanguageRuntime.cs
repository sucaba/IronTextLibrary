using System;
using System.IO;
using IronText.Logging;
using IronText.Reflection;

namespace IronText.Runtime
{
    public interface ILanguageRuntime
    {
        /// <summary>
        /// Language grammar
        /// </summary>
        Grammar        Grammar { get; }

        /// <summary>
        /// Determines whether parsing algorithm being used is deterministic.
        /// </summary>
        bool           IsDeterministic { get; }

        object         CreateDefaultContext();

        IProducer<StackNode> CreateActionProducer(object context);

        IScanner       CreateScanner(object context, TextReader input, string document, ILogging logging = null);

        IPushParser    CreateParser<TNode>(IProducer<TNode> producer, ILogging logging = null);

        int            Identify(string literal);

        int            Identify(Type symbolType);
    }

    public static class LanguageExtensions
    {
        public static Msg Literal(this ILanguageRuntime @this, string literal)
        {
            var id = @this.Identify(literal);
            return new Msg(id, null, Loc.Unknown);
        }

        public static Msg Symbol<T>(this ILanguageRuntime @this) where T : new()
        {
            return @this.Symbol(new T());
        }

        public static Msg Symbol<T>(this ILanguageRuntime @this, T value)
        {
            var id = @this.IdentifySymbolValue(value);
            return new Msg(id, value, Loc.Unknown);
        }

        public static int IdentifySymbolValue(this ILanguageRuntime @this, object value)
        {
            if (value == null)
            {
                return PredefinedTokens.Eoi;
            }

            return @this.Identify(value.GetType());
        }
    }
}
