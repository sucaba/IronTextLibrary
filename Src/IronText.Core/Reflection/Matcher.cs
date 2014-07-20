using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class Matcher : IndexableObject<IGrammarScope>
    {
        [NonSerialized]
        private readonly Joint _joint;

        public Matcher(
            string           pattern,
            SymbolBase       outcome        = null,
            Disambiguation   disambiguation = Disambiguation.Undefined)
            : this(
                ScanPattern.CreateRegular(pattern),
                outcome,
                disambiguation)
        {
        }

        public Matcher(
            ScanPattern      pattern,
            SymbolBase       outcome        = null,
            Disambiguation   disambiguation = Disambiguation.Undefined)
        {
            this.Pattern       = pattern;
            this.Outcome       = outcome;

            if (disambiguation == Disambiguation.Undefined)
            {
                this.Disambiguation = pattern.DefaultDisambiguation;
            }
            else
            {
                this.Disambiguation = disambiguation;
            }

            this._joint = new Joint();
        }

        public ScanPattern      Pattern         { get; private set; }

        public Disambiguation   Disambiguation  { get; private set; }

        public SymbolBase       Outcome         { get; private set; }

        public Joint            Joint           { get { return _joint; } }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Outcome, Pattern);
        }
    }
}
