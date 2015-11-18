﻿using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace IronText.Misc
{
    internal static class ByteSerialization
    {
        public static T DeserializeBytes<T>(byte[] data)
        {
            return (T)DeserializeBytes(data);
        }

        public static object DeserializeBytes(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (var stream = new MemoryStream(data))
            using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(decompressStream);
            }
        }
    }
}