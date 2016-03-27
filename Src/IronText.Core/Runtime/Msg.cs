using IronText.Logging;
using System;

namespace IronText.Runtime
{
    public class MsgData
    {
        public readonly int    Token;
        public readonly int    Action;
        public readonly string Text;
        public object          ExternalValue;

        /// <summary>
        /// Alternative message information for Shrodinger's token
        /// </summary>
        public MsgData  NextAlternative;

        public MsgData(int token, string text, int action)
        {
            Token  = token;
            Action = action;
            Text   = text;
        }

        public MsgData(int token, string text, object externalValue)
        {
            Token = token;
            Action = -1;
            Text = text;
            ExternalValue = externalValue;
        }
    }

    public sealed class Msg : MsgData, IEquatable<Msg>
    {
        /// <summary>
        /// Envelope Id. It can be either token ID or ambiguous token ID.
        /// </summary>
        public readonly int    AmbToken;

        /// <summary>
        /// Line, column based location for a human
        /// </summary>
        public readonly HLoc   HLocation;

        public Msg(int token, string text, object value, HLoc hLocation)
            : base(token, text, value)
        {
            this.AmbToken = token;
            this.HLocation = hLocation;
        }

        public Msg(int token, string text, int action, HLoc hLocation)
            : base(token, text, action)
        {
            this.AmbToken = token;
            this.HLocation = hLocation;
        }

        internal Msg(int ambToken, int token, int action, string text, HLoc hLocation)
            : base(token, text, action)
        {
            this.AmbToken = ambToken;
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
                && Text == other.Text
                && HLocation == other.HLocation
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return AmbToken ^ HLocation.FirstColumn ^ HLocation.LastLine;
            }
        }

        public override string ToString()
        {
            return string.Format("<Msg Id={0}, Text={1}, Loc={2}>", AmbToken, Text, HLocation);
        }
    }
}
