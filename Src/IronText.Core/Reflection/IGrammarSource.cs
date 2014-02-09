
namespace IronText.Reflection
{
    public interface IGrammarSource
    {
        /// <summary>
        /// Display language name
        /// </summary>
        string LanguageName      { get; }

        /// <summary>
        /// Unique language name
        /// </summary>
        string FullLanguageName  { get; }

        /// <summary>
        /// Grammar defintion type name or file name containing grammar.
        /// </summary>
        string Origin            { get; }

        /// <summary>
        /// Assembly qualified type name of a grammar builder
        /// </summary>
        string BuilderTypeName   { get; }
    }
}
