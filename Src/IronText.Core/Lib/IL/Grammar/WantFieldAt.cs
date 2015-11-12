using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldAt : ClassSyntax
    {
        [ParseGet("at", "auto")]
        WantFieldInit HasRVA { get; }
    }
}
