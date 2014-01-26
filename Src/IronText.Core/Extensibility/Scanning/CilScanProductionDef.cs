using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    internal abstract class CilScanRule : ICilScanRule, ICilBootstrapScanRule
    {
        public CilScanRule()
        {
            this.AllOutcomes = new List<CilSymbolRef>();
        }
        
        public MethodInfo           DefiningMethod         { get; set; }

        public Disambiguation       Disambiguation         { get; set; }

        public string               Pattern                { get; set; }

        internal string             BootstrapPattern       { get; set; }

        public string               LiteralText            { get; set; }

        public CilScanActionBuilder Builder                { get; set; }

        public Type                 NextModeType           { get; set; }

        public CilSymbolRef         MainOutcome            { get; set; }

        public List<CilSymbolRef>   AllOutcomes            { get; private set; }

        string ICilBootstrapScanRule.BootstrapPattern      { get { return BootstrapPattern; } }

        public override string ToString()
        {
            var outcomeName = ((object)AllOutcomes.FirstOrDefault() ?? "void").ToString();
            return string.Format("{0} -> {1}", outcomeName, Pattern);
        }
    }
}
