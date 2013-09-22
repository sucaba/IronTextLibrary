using IronText.Framework;

namespace CSharpParser
{
    public static class CsPreprocessor
    {
        [Scan("'#' ~('\r' | '\n' | u0085 | u2028 | u2029)*")]
        public static void Preprocessor() { }
    }
}
