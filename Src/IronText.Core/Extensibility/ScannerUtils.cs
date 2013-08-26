using System.Linq;

namespace IronText.Extensibility
{
    internal static class ScannerUtils
    {
        public static string Escape(string value)
        {
            return "'" + string.Concat(value.Select(EscapeChar)) + "'";
        }

        private static string EscapeChar(char value)
        {
            switch (value)
            {
                case '\n': return @"\n";
                case '\r': return @"\r";
                case '\'': return @"\'";
                case '\\': return @"\\";
                case '\0': return @"\0";
            }

            return new string(value, 1);
        }

    }
}
