using System;
using System.IO;

namespace IronText.Framework
{
    public interface ILanguage
    {
        LanguageName Name { get; }

        BnfGrammar  Grammar { get; }

        bool IsAmbiguous { get; }

        object CreateDefaultContext();

        IProducer<Msg> CreateActionProducer(object context);

        IScanner CreateScanner(object context, TextReader input, string document, ILogging logging = null);

        IPushParser CreateParser<TNode>(IProducer<TNode> producer, ILogging logging = null);

        int Identify(string literal);

        int Identify(Type tokenType);

        void Heatup();
    }

    public static class LanguageExtensions
    {
        public static Msg Literal(this ILanguage @this, string keyword)
        {
            var id = @this.Identify(keyword);
            return new Msg { Id = id, Value = null };
        }

        public static Msg Token<T>(this ILanguage @this) where T : new()
        {
            return @this.Token(new T());
        }

        public static Msg Token<T>(this ILanguage @this, T value)
        {
            var id = @this.IdentifyTokenValue(value);
            return new Msg(id, value, Loc.Unknown);
        }

        public static int IdentifyTokenValue(this ILanguage @this, object token)
        {
            if (token == null)
            {
                return BnfGrammar.Eoi;
            }

            return @this.Identify(token.GetType());
        }
    }
}
