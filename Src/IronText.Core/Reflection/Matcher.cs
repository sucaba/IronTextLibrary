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
            ITerminal        outcome        = null,
            Disambiguation?  disambiguation = null)
            : this(
                ScanPattern.CreateRegular(pattern),
                outcome,
                disambiguation)
        {
        }

        public Matcher(
            ScanPattern      pattern,
            ITerminal        outcome        = null,
            Disambiguation?  disambiguation = null)
        {
            this.Pattern       = pattern;
            this.Outcome       = outcome;

            if (!disambiguation.HasValue)
            {
                this.Disambiguation = pattern.DefaultDisambiguation;
            }
            else
            {
                this.Disambiguation = disambiguation.Value;
            }

            this._joint = new Joint();
        }

        public ScanPattern      Pattern         { get; private set; }

        public Disambiguation   Disambiguation  { get; private set; }

        public ITerminal        Outcome         { get; private set; }

        public Joint            Joint           { get { return _joint; } }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Outcome, Pattern);
        }
    }
}
