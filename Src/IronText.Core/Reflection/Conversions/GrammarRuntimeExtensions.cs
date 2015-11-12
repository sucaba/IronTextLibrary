using IronText.Runtime;

namespace IronText.Reflection
{
    internal static class GrammarRuntimeExtensions
    {

        public static Grammar GetGrammar(this Interpreter interpreter) 
        { 
            return interpreter.LanguageRuntime.GetGrammar();
        }

        public static Grammar GetGrammar(this ILanguageRuntime language)
        {
            var internals = (ILanguageInternalRuntime)language;
            return (Grammar)internals.GetSourceGrammar();
        }
    }
}
