using System;

namespace IronText.Build
{
    public interface ILogger
    {
        void LogError(string fmt, params object[] args);
        void LogErrorFromException(Exception e);
        void LogWarning(string fmt, params object[] args);
        void LogMessage(string fmt, params object[] args);
        void LogVerbose(string fmt, params object[] args);
    }
}
