using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronText.Reflection.Managed
{
    public class CilMatcher
    {
        public CilMatcher()
        {
            this.AllOutcomes = new List<CilSymbolRef>();
        }

        internal CilMatcher(Type outcomeType)
            : this()
        {
            var outcome = CilSymbolRef.Create(outcomeType);
            MainOutcome = outcome;
            AllOutcomes.Add(outcome);
        }

        public CilContextRef        Context                { get; set; }
        
        public MethodInfo           DefiningMethod         { get; set; }

        public Disambiguation       Disambiguation         { get; set; }

        public ScanPattern          Pattern                { get; set; }

        public CilScanActionBuilder ActionBuilder          { get; set; }

        public Type                 NextConditionType      { get; set; }

        public CilSymbolRef         MainOutcome            { get; set; }

        public List<CilSymbolRef>   AllOutcomes            { get; private set; }

        internal Type               BootstrapResultType
        {
            get
            {
                return (from outcome in AllOutcomes
                       where outcome.Type != null
                       select outcome.Type)
                       .FirstOrDefault();
            }
        }

        public override string ToString()
        {
            var outcomeName = ((object)AllOutcomes.FirstOrDefault() ?? "void").ToString();
            return string.Format("{0} -> {1}", outcomeName, Pattern);
        }
    }
}
