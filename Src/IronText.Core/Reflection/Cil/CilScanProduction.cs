using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;
using IronText.Reflection;

namespace IronText.Reflection.Managed
{
    public class CilScanProduction
    {
        public CilScanProduction()
        {
            this.AllOutcomes = new List<CilSymbolRef>();
        }

        internal CilScanProduction(Type outcomeType)
            : this()
        {
            var outcome = CilSymbolRef.Typed(outcomeType);
            MainOutcome = outcome;
            AllOutcomes.Add(outcome);
        }
        
        public MethodInfo           DefiningMethod         { get; set; }

        public Disambiguation       Disambiguation         { get; set; }

        public ScanPattern          Pattern                { get; set; }

        public CilScanActionBuilder ActionBuilder          { get; set; }

        public Type                 NextModeType           { get; set; }

        public CilSymbolRef         MainOutcome            { get; set; }

        public List<CilSymbolRef>   AllOutcomes            { get; private set; }

        internal Type               BootstrapResultType
        {
            get
            {
                return (from outcome in AllOutcomes
                       where outcome.TokenType != null
                       select outcome.TokenType)
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
