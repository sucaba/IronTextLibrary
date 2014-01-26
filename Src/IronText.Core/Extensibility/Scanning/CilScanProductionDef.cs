using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    public class CilScanRule
    {
        public CilScanRule()
        {
            this.AllOutcomes = new List<CilSymbolRef>();
        }
        
        public MethodInfo           DefiningMethod         { get; set; }

        public Disambiguation       Disambiguation         { get; set; }

        public ScanPattern          ScanPattern            { get; set; }

        internal string             BootstrapPattern       { get; set; }

        public CilScanActionBuilder Builder                { get; set; }

        public Type                 NextModeType           { get; set; }

        public CilSymbolRef         MainOutcome            { get; set; }

        public List<CilSymbolRef>   AllOutcomes            { get; private set; }

        public override string ToString()
        {
            var outcomeName = ((object)AllOutcomes.FirstOrDefault() ?? "void").ToString();
            return string.Format("{0} -> {1}", outcomeName, ScanPattern);
        }
    }
}
