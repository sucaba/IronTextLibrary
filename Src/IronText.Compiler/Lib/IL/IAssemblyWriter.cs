using System.IO;

namespace IronText.Lib.IL
{
    public interface IAssemblyWriter
    {
        void Write(string path);

        void Write(Stream stream);
    }
}
