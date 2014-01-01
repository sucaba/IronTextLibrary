using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class ScanProduction : IndexableObject<IEbnfContext>
    {
        public string Literal { get; set; }

        public string Pattern { get; set; }

        public Disambiguation Disambiguation { get; set; }

        public SymbolBase Outcome { get; set; }

        public ScanCondition Condition { get; set; }

        public ScanCondition NextCondition { get; set; }
    }
}
