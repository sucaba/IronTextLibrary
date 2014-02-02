using System.IO;

namespace IronText.Reflection
{
    public interface IGrammarTextWriter
    {
        void Write(Grammar grammar, TextWriter output);
    }
}
