using IronText.Lib.IL;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Generates IL code for creating <see cref="Grammar"/> instance 
    /// </summary>
    public class CilByteGenerator<T>
    {
        private T obj;

        public CilByteGenerator(T obj)
        {
            this.obj = obj;
        }

        public EmitSyntax Build(EmitSyntax emit)
        {
            return emit.With<DataStorage>().Load(GetGrammarBytes());
        }

        private byte[] GetGrammarBytes()
        {
            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                using (var compressStream = new DeflateStream(stream, CompressionMode.Compress, true))
                {
                    formatter.Serialize(compressStream, obj);
                }

                return stream.ToArray();
            }
        }
    }
}
