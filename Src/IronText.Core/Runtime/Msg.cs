using System;

namespace IronText.Framework
{
    public class MsgData
    {
        public readonly int    TokenId;
        public readonly object Value;

        /// <summary>
        /// Alternative message information for Shrodinger's token
        /// </summary>
        public MsgData  Next;

        public MsgData(int tokenId, object value)
        {
            TokenId = tokenId;
            Value = value;
        }
    }

    public sealed class Msg : MsgData, IEquatable<Msg>
    {
        /// <summary>
        /// Envelope Id
        /// </summary>
        public readonly int    Id;

        /// <summary>
        /// Location for an automatic processing
        /// </summary>
        public readonly Loc    Location;

        /// <summary>
        /// Line, column based location for a human
        /// </summary>
        public readonly HLoc   HLocation;

        public Msg(int tokenId, object value, Loc location, HLoc hLocation = default(HLoc))
            : this(tokenId, tokenId, value, location, hLocation)
        {
        }

        public Msg(int id, int tokenId, object value, Loc location, HLoc hLocation = default(HLoc))
            : base(tokenId, value)
        {
            this.Id = id;
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
            return Id == other.Id
                && Location == other.Location
                && object.Equals(Value, other.Value)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Id + Location.Position;
            }
        }

        public override string ToString()
        {
            return string.Format("<Msg Id={0}, Val={1}, Loc={2}>", Id, Value, Location);
        }
    }
}
