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

        public static Chr BootstrapParse(string text)
        {
            if (text.StartsWith("u"))
            {
                int ch = (Hex(text[1]) << 12)
                       + (Hex(text[2]) << 8)
                       + (Hex(text[3]) << 4)
                       + Hex(text[4])
                       ;
                return new Chr((char)ch);
            }

            return Parse(text);
        }
        
        internal static int Hex(char ch)
        {
            switch (ch)
            {
                case '0': return 0x0;
                case '1': return 0x1;
                case '2': return 0x2;
                case '3': return 0x3;
                case '4': return 0x4;
                case '5': return 0x5;
                case '6': return 0x6;
                case '7': return 0x7;
                case '8': return 0x8;
                case '9': return 0x9;
                case 'A':
                case 'a': return 0xa;
                case 'B':
                case 'b': return 0xb;
                case 'C':
                case 'c': return 0xc;
                case 'D':
                case 'd': return 0xd;
                case 'E':
                case 'e': return 0xe;
                case 'F':
                case 'f': return 0xf;
                default: return -1;
            }
        }

        public Chr(char ch)
        {
            this.Char = ch;
        }

        public char Char { get; private set; }
    }
}
