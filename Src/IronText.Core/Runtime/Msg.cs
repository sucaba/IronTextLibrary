using System;
using IronText.Logging;

namespace IronText.Runtime
{
    public class MsgData
    {
        public readonly int    Token;
        public object          Value;
        public readonly int    Action;
        public readonly string Text;

        /// <summary>
        /// Alternative message information for Shrodinger's token
        /// </summary>
        public MsgData  Next;

        public MsgData(int token, object value, int action, string text)
        {
            Token  = token;
            Value  = value;
            Action = action;
            Text   = text;
        }
    }

    public sealed class Msg : MsgData, IEquatable<Msg>
    {
        /// <summary>
        /// Envelope Id. It can be either token ID or ambiguous token ID.
        /// </summary>
        public readonly int    AmbToken;

        /// <summary>
        /// Location for an automatic processing
        /// </summary>
        public readonly Loc    Location;

        /// <summary>
        /// Line, column based location for a human
        /// </summary>
        public readonly HLoc   HLocation;

        public Msg(int token, object value, Loc location, HLoc hLocation = default(HLoc))
            : this(token, value, -1, null, location, hLocation)
        {
        }

        public Msg(int token, object value, int action, string text, Loc location, HLoc hLocation = default(HLoc))
            : this(token, token, value, action, text, location, hLocation)
        {
        }

        public Msg(int ambToken, int token, object value, int action, string text, Loc location, HLoc hLocation = default(HLoc))
            : base(token, value, action, text)
        {
            this.AmbToken  = ambToken;
            this.Location  = location;
            this.HLocation = hLocation;
        }

        public MsgData FirstData { get { return this; } }

        public override bool Equals(object obj)
        {
            var casted = (Msg)obj;
            return Equals(casted);
        }

        public bool Equals(Msg other)
        {
            return AmbToken == other.AmbToken
                && Location == other.Location
                && object.Equals(Value, other.Value)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return AmbToken + Location.Position;
            }
        }

        public override string ToString()
        {
            return string.Format("<Msg Id={0}, Val={1}, Loc={2}>", AmbToken, Value, Location);
        }
    }
}
