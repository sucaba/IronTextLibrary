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
            this.SymbolTypes = new List<Type>();
        }

        public MethodInfo           DefiningMember         { get; set; }

        public Disambiguation       Disambiguation         { get; set; }

        public string               Pattern                { get; set; }

        internal string             BootstrapRegexPattern  { get; set; }

        public string               LiteralText            { get; set; }

        public List<Type>           SymbolTypes            { get; private set; }

        public CilScanActionBuilder ActionBuilder          { get; set; }

        public Type                 NextModeType           { get; set; }
        
        public abstract CilSymbolRef MainTokenRef { get; }

        public abstract IEnumerable<CilSymbolRef[]> GetTokenRefGroups();

        string ICilBootstrapScanRule.BootstrapRegexPattern { get { return BootstrapRegexPattern; } }

        public override string ToString()
        {
            if (DefiningMember != null)
            {
                return DefiningMember.ToString();
            }
            else if (LiteralText != null)
            {
                return LiteralText;
            }

            return base.ToString();
        }
    }
}
