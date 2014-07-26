using System;
using System.Collections.Generic;
using IronText.Lib.Ctem;
using IronText.Lib.IL;
using IronText.Reflection;
using IronText.Lib.Shared;
using IronText.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Generates IL code for creating <see cref="Grammar"/> instance 
    /// </summary>
    public class GrammarSerializer
    {
        private Grammar grammar;

        public GrammarSerializer(Grammar grammar)
        {
            this.grammar = grammar;
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
                    formatter.Serialize(compressStream, grammar);
                }

                return stream.ToArray();
            }
        }
    }
}
