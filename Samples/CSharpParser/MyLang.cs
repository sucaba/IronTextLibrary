using IronText.Framework;

namespace Samples
{
    [Language]
    public class MyLang
    {
        public void Parse(string input)
        {
            Language.Parse(this, input);
        }

        [ParseResult]
        public string Result { get; set; }

        [Scan("'foo'")]
        public string Foo(string text) { return text; }
    }
}
