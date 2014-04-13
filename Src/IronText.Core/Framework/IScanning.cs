using IronText.Logging;

namespace IronText.Framework
{
    public interface IScanning
    {
        Loc Location { get; }

        HLoc HLocation { get; }
    }
}
