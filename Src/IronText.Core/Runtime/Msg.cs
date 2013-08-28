#define MSGCLASS

using System;
using IronText.Logging;

namespace IronText.Framework
{
#if MSGCLASS
    public class Msg
#else
    public struct Msg
#endif
        : IEquatable<Msg>
    {
        public int    Id;
        public object Value;
        public Loc    Location;
        public HLoc   HLocation;

#if MSGCLASS
        public Msg()
        {
        }
#endif
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
