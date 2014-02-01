using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class Matcher : IndexableObject<IEbnfEntities>
    {
        public Matcher(
            string         pattern,
            SymbolBase     outcome = null,
            Condition      nextCondition = null,
            Disambiguation disambiguation = Disambiguation.Undefined)
            : this(
                ScanPattern.CreateRegular(pattern),
                outcome,
                nextCondition,
                disambiguation)
        {
        }

        public Matcher(
            ScanPattern    pattern,
            SymbolBase     outcome        = null,
            Condition  nextCondition  = null,
            Disambiguation disambiguation = Disambiguation.Undefined)
        {
            this.Pattern       = pattern;
            this.Outcome       = outcome;
            this.NextCondition = nextCondition;

            if (disambiguation == Disambiguation.Undefined)
            {
                this.Disambiguation = pattern.DefaultDisambiguation;
            }
            else
            {
                this.Disambiguation = disambiguation;
            }

            this.Joint = new Joint();
        }

        public ScanPattern      Pattern         { get; private set; }

        public Disambiguation   Disambiguation  { get; private set; }

        public SymbolBase       Outcome         { get; private set; }

        public Condition    NextCondition   { get; private set; }

        public Joint Joint { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Outcome, Pattern);
        }
    }
}
