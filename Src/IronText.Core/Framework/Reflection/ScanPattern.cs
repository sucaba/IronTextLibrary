using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;

namespace IronText.Framework.Reflection
{
    public abstract class ScanPattern
    {
        public static ScanPattern CreateLiteral(string literal)
        {
            return new ScanLiteral(literal);
        }

        public static ScanPattern CreateRegular(string pattern)
        {
            return new ScanRegular(pattern);
        }

        public abstract string Literal { get; }

        public abstract string Pattern { get; }

        public abstract Disambiguation DefaultDisambiguation { get; }
    }

    sealed class ScanLiteral : ScanPattern
    {
        private readonly string literal;
        private readonly string pattern;

        public ScanLiteral(string literal)
        {
            this.literal = literal;
            this.pattern = ScannerUtils.Escape(literal);
        }

        public override string Literal { get { return literal; } }

        public override string Pattern { get { return pattern; } }

        public override Disambiguation DefaultDisambiguation { get { return Disambiguation.Exclusive; } }
    }

    sealed class ScanRegular : ScanPattern
    {
        private readonly string pattern;

        public ScanRegular(string pattern)
        {
            this.pattern = pattern;
        }

        public override string Literal { get { return null; } }

        public override string Pattern { get { return pattern; } }

        public override Disambiguation DefaultDisambiguation { get { return Disambiguation.Contextual; } }
    }
}
