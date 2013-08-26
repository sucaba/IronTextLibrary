using System;

namespace IronText.MetadataCompiler
{
    class MetadataException : Exception
    {
        public MetadataException(string message)
            : base(message)
        {
        }

        public MetadataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
