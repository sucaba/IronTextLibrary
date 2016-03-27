using IronText.Logging;

namespace IronText.Framework
{
    public sealed class NullParsing : IParsing
    {
        public static IParsing Instance { get {  return new NullParsing(); } }

        Loc IParsing.Location { get { return Loc.Unknown; } }
    }
}
