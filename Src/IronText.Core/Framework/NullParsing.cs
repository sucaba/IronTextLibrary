using IronText.Logging;

namespace IronText.Framework
{
    public sealed class NullParsing : IParsing
    {
        public static IParsing Instance { get {  return new NullParsing(); } }

        HLoc IParsing.HLocation { get { return HLoc.Unknown; } }
    }
}
