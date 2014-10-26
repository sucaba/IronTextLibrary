using IronText.Logging;

namespace IronText.Runtime
{
    public class ActionNode
    {
        public readonly int    Token;
        public readonly object Value;
        public readonly Loc    Location;
        public readonly HLoc   HLocation;
        public readonly SListOfInhProp InheritedProperties;

        public ActionNode(int token, object value, Loc loc, HLoc hLoc, SListOfInhProp inh = null)
        {
            this.Token      = token;
            this.Value      = value;
            this.Location   = loc;
            this.HLocation  = hLoc;
            this.InheritedProperties = inh;
        }
    }

    public class SListOfInhProp
    {
        public SListOfInhProp Next     { get; set; }

        public int            InhIndex { get; set; }

        public object         Value    { get; set; }
    }
}