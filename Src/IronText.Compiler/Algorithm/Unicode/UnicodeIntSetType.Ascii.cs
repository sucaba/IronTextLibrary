
namespace IronText.Algorithm
{
    public partial class UnicodeIntSetType
    {
        public const int AsciiMinValue = 0x00;
        public const int AsciiMaxValue = 0x7F;

        public const int NewLine        = 0x0A;
        public const int CarriageReturn = 0x0D;

        public static readonly IntInterval AsciiInterval = new IntInterval(AsciiMinValue, AsciiMaxValue);

        /// ASCII letters and digits
        public IntSet AsciiAlnum { get; private set; }

        /// ASCII letters
        public IntSet AsciiAlpha { get; private set; }

        /// ASCII characters
        public IntSet Ascii      { get; private set; }

        /// ASCII widthful whitespace: space and tab
        public IntSet AsciiBlank { get; private set; }

        /// “control” characters: ASCII 0 to 32
        public IntSet AsciiCntrl { get; private set; }

        /// ASCII digits
        public IntSet AsciiDigit { get; private set; }

        /// ASCII characters that use ink
        public IntSet AsciiGraph { get; private set; }

        /// ASCII lower-case letters
        public IntSet AsciiLower { get; private set; }
        
        /// ASCII ink-users plus widthful whitespace
        public IntSet AsciiPrint { get; private set; }

        /// [!"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~]
        public IntSet AsciiPunct { get; private set; }

        /// ASCII whitespace
        public IntSet AsciiSpace { get; private set; }

        /// ASCII upper-case letters
        public IntSet AsciiUpper { get; private set; }

        /// ASCII letters and _
        public IntSet AsciiWord { get; private set; }

        /// ASCII hex digits
        public IntSet AsciiXDigit { get; private set; }

        private void InitAscii()
        {
            Ascii      = Range(AsciiMinValue, AsciiMaxValue);

            AsciiBlank = Of(" \t");

            AsciiCntrl = Range(0, 0x1F).Union(Of(AsciiMaxValue));

            AsciiDigit = Range('0', '9');

            AsciiGraph = Range(0x21, 0x7E);

            AsciiLower = Range('a', 'z');
            AsciiUpper = Range('A','Z');
            AsciiAlpha = AsciiLower.Union(AsciiUpper);
            AsciiAlnum = AsciiAlpha.Union(AsciiDigit);

            AsciiPunct = Of(@"!""#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~");

            AsciiPrint = Range(0x20, 0x7E);

            AsciiSpace = Of(" \t\r\n\v\f");

            AsciiWord = AsciiAlnum.Union(Of('_'));

            AsciiXDigit = AsciiDigit.Union(Range('A', 'F')).Union(Range('a', 'f'));
        }
    }
}
