
namespace IronText.Framework
{
    public interface IScanning
    {
        Loc Location { get; }

        HLoc HLocation { get; }

        /// <summary>
        /// Do not produce <see cref="Msg"/> from the currently scanned token.
        /// </summary>
        void Skip();
    }
}
