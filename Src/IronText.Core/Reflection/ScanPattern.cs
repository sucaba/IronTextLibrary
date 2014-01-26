using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IronText.Extensibility;

namespace IronText.Reflection
{
    public abstract class ScanPattern
    {
        public static ScanPattern CreateLiteral(string literal)
        {
            return new ScanLiteral(literal);
        }

        public static ScanPattern CreateRegular(string pattern)
        {
            return new ScanRegular(pattern, null);
        }

        internal static ScanPattern CreateRegular(string pattern, string regexPattern)
        {
            return new ScanRegular(pattern, regexPattern);
        }

        public bool IsLiteral { get; protected set; }

        public virtual string Literal { get { return null; } }

        public virtual string Pattern { get { return null; } }

        internal virtual string BootstrapPattern { get { return null; } }

        public abstract Disambiguation DefaultDisambiguation { get; }

        public override string ToString() { return Pattern; }
    }

    sealed class ScanLiteral : ScanPattern
    {
        private readonly string literal;
        private readonly string pattern;
        private readonly string regexPattern;

        public ScanLiteral(string literal)
        {
            this.IsLiteral = true;

            this.literal      = literal;
            this.pattern      = ScannerUtils.Escape(literal);
            this.regexPattern = Regex.Escape(literal);
        }

        public override string Literal { get { return literal; } }

        public override string Pattern { get { return pattern; } }

        internal override string BootstrapPattern { get { return regexPattern; } }

        public override Disambiguation DefaultDisambiguation { get { return Disambiguation.Exclusive; } }
    }

    sealed class ScanRegular : ScanPattern
    {
        private readonly string pattern;
        private readonly string regexPattern;

        public ScanRegular(string pattern, string regexPattern)
        {
            this.IsLiteral = false;

            this.pattern      = pattern;
            this.regexPattern = regexPattern;
        }

        public override string Pattern { get { return pattern; } }

        internal override string BootstrapPattern { get { return regexPattern; } }

        public override Disambiguation DefaultDisambiguation { get { return Disambiguation.Contextual; } }
    }
}
