using System.IO;
using System.Text;

namespace IronText.Reflection
{
    public interface IGrammarTextWriter
    {
        void Write(Grammar grammar, TextWriter output);
    }
}
