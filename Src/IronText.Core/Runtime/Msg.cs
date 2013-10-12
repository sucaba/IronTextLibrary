using System;

namespace IronText.Framework
{
    public class Msg : IEquatable<Msg>
    {
        public readonly int Id;

        public readonly object Value;

        public readonly Loc    Location;

        public readonly HLoc   HLocation;

        public Msg(int id, object value, Loc location, HLoc hLocation = default(HLoc))
        {
            this.Id        = id;
            this.Value     = value;
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
