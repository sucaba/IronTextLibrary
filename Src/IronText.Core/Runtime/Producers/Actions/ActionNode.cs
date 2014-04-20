using IronText.Logging;

namespace IronText.Runtime
{
    public class ActionNode
    {
        public readonly int    Token;
        public readonly object Value;
        public readonly Loc    Location;
        public readonly HLoc   HLocation;

        public ActionNode(int token, object value, Loc loc, HLoc hLoc)
        {
            this.Token      = token;
            this.Value      = value;
            this.Location   = loc;
            this.HLocation  = hLoc;
        }
    }
}
