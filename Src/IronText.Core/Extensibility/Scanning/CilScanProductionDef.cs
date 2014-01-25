using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    internal abstract class CilScanRule : ICilScanRule, ICilBootstrapScanRule
    {
        public MethodInfo           DefiningMethod         { get; set; }

        public Disambiguation       Disambiguation         { get; set; }

        public string               Pattern                { get; set; }

        internal string             BootstrapPattern       { get; set; }

        public string               LiteralText            { get; set; }

        public Type                 SymbolType             { get; set; }

        public CilScanActionBuilder Builder                { get; set; }

        public Type                 NextModeType           { get; set; }


        public abstract CilSymbolRef MainOutcome { get; set; }

        public abstract IEnumerable<CilSymbolRef> AllOutcomes { get; }

        string ICilBootstrapScanRule.BootstrapPattern { get { return BootstrapPattern; } }

        public override string ToString()
        {
            if (DefiningMethod != null)
            {
                return DefiningMethod.ToString();
            }
            else if (LiteralText != null)
            {
                return LiteralText;
            }

            return base.ToString();
        }
    }
}
