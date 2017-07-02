using IronText.Logging;
using System;
using System.Runtime.Serialization;

namespace IronText.DI
{
    [Serializable]
    class InvalidDependencyException : Exception, INonLoggable
    {
        public InvalidDependencyException()
        {
        }

        public InvalidDependencyException(string message) : base(message)
        {
        }

        public InvalidDependencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidDependencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}