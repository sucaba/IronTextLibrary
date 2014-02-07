using System.Linq;

namespace IronText.Algorithm
{
    /// <summary>
    /// Represents set of code-points (values in range 0 .. 0x10FFFF).
    /// </summary>
    public partial class UnicodeIntSetType : SparseIntSetType
    {
        public const int UnicodeMaxValue     = 0x10FFFF;

        public const char NextLineChar           = '\u0085';
        public const char LineSeparatorChar      = '\u2028';
        public const char ParagraphSeparatorChar = '\u2029';

        public static readonly new UnicodeIntSetType Instance = new UnicodeIntSetType();

        public static readonly IntInterval UnicodeInterval = new IntInterval(0, UnicodeMaxValue);

        protected UnicodeIntSetType()
        {
            InitMainGeneralCategories();
            InitAscii();

            Symbol = MathSymbol
                .Union(CurrencySymbol)
                .Union(ModifierSymbol)
                .Union(OtherSymbol);
            Punctuation = ConnectorPunctuation
                .Union(DashPunctuation)
                .Union(OpenPunctuation)
                .Union(ClosePunctuation)
                .Union(InitialQuotePunctuation)
                .Union(FinalQuotePunctuation)
                .Union(OtherPunctuation);
            Other = Control
                .Union(Format)
                .Union(Surrogate)
                .Union(PrivateUse)
                .Union(Unassigned);
            Separator = SpaceSeparator
                .Union(LineSeparator)
                .Union(ParagraphSeparator);
            Number = DecimalDigitNumber
                .Union(LetterNumber)
                .Union(OtherNumber);
            Mark = NonSpacingMark
                .Union(SpacingCombiningMark)
                .Union(EnclosingMark);
            CasedLetter = UppercaseLetter
                .Union(LowercaseLetter)
                .Union(TitlecaseLetter);
            Letter = CasedLetter
                .Union(ModifierLetter)
                .Union(OtherLetter);
        }

        public override int MaxValue { get { return 0x10FFFF; } }

        public IntSet Of(string text) { return Of(text.Select(ch => (int)ch)); }

        /// <summary>
        ///     Uppercase letter. Signified by the Unicode designation "Lu" (letter, uppercase).
        /// </summary>
        public IntSet UppercaseLetter { get; private set; }

        /// <summary>
        ///     Lowercase letter. Signified by the Unicode designation "Ll" (letter, lowercase).
        /// </summary>
        public IntSet LowercaseLetter { get; private set; }

        /// <summary>
        ///     Titlecase letter. Signified by the Unicode designation "Lt" (letter, titlecase).
        /// </summary>
        public IntSet TitlecaseLetter { get; private set; }

        /// <summary>
        ///     Lu | Ll | Lt
        /// </summary>
        public IntSet CasedLetter { get; private set; }

        /// <summary>
        ///     Modifier letter character, which is free-standing spacing character that
        ///     indicates modifications of a preceding letter. Signified by the Unicode designation
        ///     "Lm" (letter, modifier). 
        /// </summary>
        public IntSet ModifierLetter { get; private set; }

        /// <summary>
        ///     Letter that is not an uppercase letter, a lowercase letter, a titlecase letter,
        ///     or a modifier letter. Signified by the Unicode designation "Lo" (letter,
        ///     other). 
        /// </summary>
        public IntSet OtherLetter { get; private set; }

        /// <summary>
        ///     Lu | Ll | Lt | Lm | Lo
        /// </summary>
        public IntSet Letter { get; private set; }

        /// <summary>
        ///     Nonspacing character that indicates modifications of a base character. Signified
        ///     by the Unicode designation "Mn" (mark, nonspacing). 
        /// </summary>
        public IntSet NonSpacingMark { get; private set; }

        /// <summary>
        ///     Spacing character that indicates modifications of a base character and affects
        ///     the width of the glyph for that base character. Signified by the Unicode
        ///     designation "Mc" (mark, spacing combining). 
        /// </summary>
        public IntSet SpacingCombiningMark { get; private set; }

        /// <summary>
        ///     Enclosing mark character, which is a nonspacing combining character that
        ///     surrounds all previous characters up to and including a base character. Signified
        ///     by the Unicode designation "Me" (mark, enclosing). 
        /// </summary>
        public IntSet EnclosingMark { get; private set; }

        /// <summary>
        /// Mn | Mc | Me
        /// </summary>
        public IntSet Mark { get; private set; }

        /// <summary>
        ///     Decimal digit character, that is, a character in the range 0 through 9. Signified
        ///     by the Unicode designation "Nd" (number, decimal digit). 
        /// </summary>
        public IntSet DecimalDigitNumber { get; private set; }

        /// <summary>
        ///     Number represented by a letter, instead of a decimal digit, for example,
        ///     the Roman numeral for five, which is "V". The indicator is signified by the
        ///     Unicode designation "Nl" (number, letter). 
        /// </summary>
        public IntSet LetterNumber { get; private set; }

        /// <summary>
        ///     Number that is neither a decimal digit nor a letter number, for example,
        ///     the fraction 1/2. The indicator is signified by the Unicode designation "No"
        ///     (number, other). 
        /// </summary>
        public IntSet OtherNumber { get; private set; }

        /// <summary>
        ///     Nd | Nl | No
        /// </summary>
        public IntSet Number { get; private set; }

        /// <summary>
        ///     Space character, which has no glyph but is not a control or format character.
        ///     Signified by the Unicode designation "Zs" (separator, space). The value is
        ///     11.
        /// </summary>
        public IntSet SpaceSeparator { get; private set; }

        /// <summary>
        ///     Character that is used to separate lines of text. Signified by the Unicode
        ///     designation "Zl" (separator, line). 
        /// </summary>
        public IntSet LineSeparator { get; private set; }

        /// <summary>
        ///     Character used to separate paragraphs. Signified by the Unicode designation
        ///     "Zp" (separator, paragraph). 
        /// </summary>
        public IntSet ParagraphSeparator { get; private set; }

        /// <summary>
        ///     Zs | Zl | Zp
        /// </summary>
        public IntSet Separator { get; private set; }

        /// <summary>
        ///     Control code character, with a Unicode value of U+007F or in the range U+0000
        ///     through U+001F or U+0080 through U+009F. Signified by the Unicode designation
        ///     "Cc" (other, control). 
        /// </summary>
        public IntSet Control { get; private set; }

        /// <summary>
        ///     Format character that affects the layout of text or the operation of text
        ///     processes, but is not normally rendered. Signified by the Unicode designation
        ///     "Cf" (other, format). 
        /// </summary>
        public IntSet Format { get; private set; }

        /// <summary>
        ///     High surrogate or a low surrogate character. Surrogate code values are in
        ///     the range U+D800 through U+DFFF. Signified by the Unicode designation "Cs"
        ///     (other, surrogate). 
        /// </summary>
        public IntSet Surrogate { get; private set; }

        /// <summary>
        ///     Private-use character, with a Unicode value in the range U+E000 through U+F8FF.
        ///     Signified by the Unicode designation "Co" (other, private use). 
        /// </summary>
        public IntSet PrivateUse { get; private set; }

        /// <summary>
        ///     A reserved unassigned code point or a noncharacter.
        ///     Signified by the Unicode designation "Cn".
        /// </summary>
        public IntSet Unassigned { get; private set; }

        /// <summary>
        ///     Cc | Cf | Cs | Co | Cn
        /// </summary>
        public IntSet Other { get; private set; }

        /// <summary>
        ///     Connector punctuation character that connects two characters. Signified by
        ///     the Unicode designation "Pc" (punctuation, connector). 
        /// </summary>
        public IntSet ConnectorPunctuation { get; private set; }


        /// <summary>
        ///     Dash or hyphen character. Signified by the Unicode designation "Pd" (punctuation,
        ///     dash). 
        /// </summary>
        public IntSet DashPunctuation { get; private set; }

        /// <summary>
        ///     Opening character of one of the paired punctuation marks, such as parentheses,
        ///     square brackets, and braces. Signified by the Unicode designation "Ps" (punctuation,
        ///     open). 
        /// </summary>
        public IntSet OpenPunctuation { get; private set; }

        /// <summary>
        ///     Closing character of one of the paired punctuation marks, such as parentheses,
        ///     square brackets, and braces. Signified by the Unicode designation "Pe" (punctuation,
        ///     close). 
        /// </summary>
        public IntSet ClosePunctuation { get; private set; }

        /// <summary>
        ///     Opening or initial quotation mark character. Signified by the Unicode designation
        ///     "Pi" (punctuation, initial quote). 
        /// </summary>
        public IntSet InitialQuotePunctuation { get; private set; }

        /// <summary>
        ///     Closing or final quotation mark character. Signified by the Unicode designation
        ///     "Pf" (punctuation, final quote). 
        /// </summary>
        public IntSet FinalQuotePunctuation { get; private set; }

        /// <summary>
        ///     Punctuation character that is not a connector, a dash, open punctuation,
        ///     close punctuation, an initial quote, or a final quote. Signified by the Unicode
        ///     designation "Po" (punctuation, other). 
        /// </summary>
        public IntSet OtherPunctuation { get; private set; }

        /// <summary>
        ///     Pc | Pd | Ps | Pe | Pi | Pf | Po
        /// </summary>
        public IntSet Punctuation { get; private set; }

        /// <summary>
        ///     Mathematical symbol character, such as "+" or "= ". Signified by the Unicode
        ///     designation "Sm" (symbol, math). 
        /// </summary>
        public IntSet MathSymbol { get; private set; }

        /// <summary>
        ///     Currency symbol character. Signified by the Unicode designation "Sc" (symbol,
        ///     currency). 
        /// </summary>
        public IntSet CurrencySymbol { get; private set; }

        /// <summary>
        ///     Modifier symbol character, which indicates modifications of surrounding characters.
        ///     For example, the fraction slash indicates that the number to the left is
        ///     the numerator and the number to the right is the denominator. The indicator
        ///     is signified by the Unicode designation "Sk" (symbol, modifier). The value
        ///     is 27.
        /// </summary>
        public IntSet ModifierSymbol { get; private set; }

        /// <summary>
        ///     Symbol character that is not a mathematical symbol, a currency symbol or
        ///     a modifier symbol. Signified by the Unicode designation "So" (symbol, other).
        /// </summary>
        public IntSet OtherSymbol { get; private set; }

        /// <summary>
        ///     Sm | Sc | Sk | So
        /// </summary>
        public IntSet Symbol { get; private set; }
    }
}
