using System.Text;

namespace IronText.Lib.Stem
{
    public class QStr : Sym
    {
        private const char Quote = '"';
        private const string EscapedQuote = "\\\"";

        public static QStr Parse(char[] buffer, int start, int length)
        {
            var escaped = IronText.Lib.Ctem.QStr.Unescape(buffer, start + 1, length - 2);
            return new QStr(escaped);
        }

        public QStr(string text) : base(text) { }

        protected override void DoWrite(StringBuilder output)
        {
            output.Append(Quote).Append(this.Text.Replace(Quote.ToString(), EscapedQuote)).Append(Quote);
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
