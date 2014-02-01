using IronText.Framework;
using System.Collections.Generic;
using IronText.Lib.Ctem;
using IronText.Runtime;

namespace Samples
{
    [Language]
    public class MyConfig
    {
        // Initialize instance of CtemScanner sub-context
        public MyConfig() { Scanner = new CtemScanner(); }

        public void Parse(string input)
        {
            Language.Parse(this, input);
        }

        [Outcome]
        public Dictionary<string, object> Parameters { get; set; }

        // Use relevant scan and parse rules from this vocabulary during
        // the build and use Scanner-property value as a runtime-context 
        // for these rules.
        [SubContext]
        public CtemScanner Scanner { get; private set; }

        [Produce]
        public Dictionary<string, object> Empty()
        {
            return new Dictionary<string, object>();
        }

        [Produce(null, null, "=", null)]
        public Dictionary<string, object> AddPair(
                Dictionary<string, object> items,
                string name,
                Variant variant)
        {
            items.Add(name, variant.Value);
            return items;
        }

        [Produce]
        public Variant QuotedStringValue(QStr str)
        {
            return new Variant(str.Text);
        }

        [Produce]
        public Variant NumberValue(Num num)
        {
            return new Variant(
                num.Text.Contains(".")
                 ? double.Parse(num.Text)
                 : int.Parse(num.Text));
        }
    }
}

// Wraps any config value
public class Variant
{
    public Variant(object value) { this.Value = value; }

    public object Value { get; private set; }
}
