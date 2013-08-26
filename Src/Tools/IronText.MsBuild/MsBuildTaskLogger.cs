using System;
using IronText.Build;
using Microsoft.Build.Utilities;

namespace IronText.MsBuild
{
    class MsBuildTaskLogger : ILogger
    {
        private readonly TaskLoggingHelper log;

        public MsBuildTaskLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        public void LogMessage(string fmt, params object[] args)
        {
            log.LogMessage(string.Format(fmt, args));
        }

        public void LogWarning(string fmt, params object[] args)
        {
            log.LogWarning(string.Format(fmt, args));
        }

        public void LogError(string fmt, params object[] args)
        {
            log.LogError(string.Format(fmt, args));
        }

        public void LogErrorFromException(Exception e)
        {
            log.LogErrorFromException(e);
        }

        public void LogVerbose(string fmt, params object[] args)
        {
            log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, string.Format(fmt, args));
        }
    }
}
