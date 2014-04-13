using System.Text;

namespace IronText.Lib.Ctem
{
    public class QStr
    {
        public static QStr Parse(string text)
        {
            return new QStr(Unescape(text.ToCharArray(), 1, text.Length - 2));
        }

        public QStr(string text) { Text = text; }

        public string Text { get; private set; }

        public static string Unescape(string text)
        {
            return Unescape(text.ToCharArray(), 0, text.Length);
        }

        public static string Unescape(char[] buffer, int start, int length)
        {
            var output = new StringBuilder();
            bool escaping = false;
            int last = length + start;
            for (int i = start; i != last; ++i)
            {
                char ch = buffer[i];
                if (escaping)
                {
                    escaping = false;
                    switch (ch)
                    {
                        case 'n': output.Append('\n'); break;
                        case 'r': output.Append('\r'); break;
                        case 't': output.Append('\t'); break;
                        case '0': output.Append('\0'); break;
                        default:
                            output.Append(ch);
                            break;
                    }
                }
                else
                {
                    switch (ch)
                    {
                        case '\\': escaping = true; break;
                        default: output.Append(ch); break;
                    }
                }
            }

            if (escaping)
            {
                output.Append('\\');
            }

            return output.ToString();
        }

        #region Object overrides

        /// <summary>
        /// Determine if two lexeme datums are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var syntax = obj as QStr;
            return syntax != null && syntax.Text == this.Text;
        }

        public override int GetHashCode()
        {
            return this.Text.GetHashCode();
        }

        #endregion
    }
}
