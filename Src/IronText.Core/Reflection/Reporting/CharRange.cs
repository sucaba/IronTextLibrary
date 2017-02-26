namespace IronText.Reflection.Reporting
{
    public struct CharRange
    {
        public CharRange(int first, int last)
        {
            this.First = first;
            this.Last  = last;
        }

        public int First { get; }

        public int Last  { get; }

        public override string ToString() =>
            $"{NameOfChar(First)}-{NameOfChar(Last)}";

        private static string NameOfChar(int ch)
        {
            if (ch == '\n') { return "\\n"; }
            if (ch == '\r') { return "\\r"; }
            if (ch == '\t') { return "\\t"; }
            if (ch == '\b') { return "\\b"; }
            if (ch == ' ') { return "<SPACE>"; }

            if (char.IsWhiteSpace((char)ch) || ch < 0x20 || ch > 0x7f)
            {
                return string.Format("U-{0:X4}", ch);
            }
            else
            {
                return ((char)ch).ToString();
            }
        }
    }
}
