namespace IronText.Runtime
{
    public interface ILanguageSource
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
        string GrammarOrigin     { get; }

        /// <summary>
        /// Assembly qualified type name of a grammar reader
        /// </summary>
        string GrammarReaderTypeName   { get; }
    }
}
