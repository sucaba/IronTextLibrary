using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanProduction : IndexableObject<IScanConditionContext>
    {
        public ScanProduction(
            ScanPattern    pattern,
            SymbolBase     outcome        = null,
            ScanCondition  nextCondition  = null,
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
        }

        public ScanCondition Condition          { get { return Context.Condition; } }

        public ScanPattern      Pattern         { get; private set; }

        public Disambiguation   Disambiguation  { get; private set; }

        public SymbolBase       Outcome         { get; private set; }

        public ScanCondition    NextCondition   { get; private set; }
    }
}
