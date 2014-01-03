using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanProduction : IndexableObject<IEbnfContext>
    {
        public ScanProduction(
            string         pattern,
            SymbolBase     outcome = null,
            ScanCondition  nextCondition = null,
            Disambiguation disambiguation = Disambiguation.Undefined)
            : this(
                ScanPattern.CreateRegular(pattern),
                outcome,
                nextCondition,
                disambiguation)
        {
        }

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

            this.PlatformToBinding = new TypeMap<IScanProductionBinding>();
        }

        public ScanPattern      Pattern         { get; private set; }

        public Disambiguation   Disambiguation  { get; private set; }

        public SymbolBase       Outcome         { get; private set; }

        public ScanCondition    NextCondition   { get; private set; }

        public TypeMap<IScanProductionBinding> PlatformToBinding { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Outcome, Pattern);
        }
    }
}
