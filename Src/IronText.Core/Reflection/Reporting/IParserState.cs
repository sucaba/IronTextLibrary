using System.Collections.ObjectModel;

namespace IronText.Reflection.Reporting
{
    public interface IParserState
    {
        int Index { get; }

        ReadOnlyCollection<IParserDotItem>    DotItems { get; }

        ReadOnlyCollection<IParserTransition> Transitions { get; }
    }
}
