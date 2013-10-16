using System;

namespace IronText.Framework
{
    public class MsgAlternative
    {
        public readonly int    Id;
        public readonly object Value;

        /// <summary>
        /// Alternative message information for Shrodinger's token
        /// </summary>
        public MsgAlternative  Next;

        public MsgAlternative(int id, object value)
        {
            Id = id;
            Value = value;
        }
    }

    public sealed class Msg : MsgAlternative, IEquatable<Msg>
    {
        /// <summary>
        /// Location for an automatic processing
        /// </summary>
        public readonly Loc    Location;

        /// <summary>
        /// Line, column based location for a human
        /// </summary>
        public readonly HLoc   HLocation;

        public Msg(int id, object value, Loc location, HLoc hLocation = default(HLoc))
            : base(id, value)
        {
            this.Location  = location;
            this.HLocation = hLocation;
        }

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
