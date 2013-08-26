using System.Diagnostics;
using IronText.Lib.Ctem;

namespace IronText.Lib.ScannerExpressions
{
    public class Chr
    {
        public static Chr Parse(char[] buffer, int start, int length)
        {
            var unescaped = QStr.Unescape(buffer, start + 1, length - 2);
            Debug.Assert(unescaped.Length == 1);
            return new Chr(unescaped[0]);
        }

        public static Chr Parse(string text)
        {
            var unescaped = QStr.Unescape(text.ToCharArray(), 1, text.Length - 2);
            Debug.Assert(unescaped.Length == 1);
            return new Chr(unescaped[0]);
        }

        public Chr(char ch)
        {
            this.Char = ch;
        }

        public char Char { get; private set; }
    }
}
