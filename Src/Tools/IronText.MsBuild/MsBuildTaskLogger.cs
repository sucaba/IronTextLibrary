using System;
using IronText.Build;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace IronText.MsBuild
{
    class MsBuildTaskLogger : IronText.Build.ILogger
    {
        private readonly TaskLoggingHelper log;

        public MsBuildTaskLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        public void LogMessage(string fmt, params object[] args)
        {
            string msg = args.Length == 0 ? fmt : string.Format(fmt, args);
            log.LogMessage(MessageImportance.Normal, msg);
        }

        public void LogWarning(string fmt, params object[] args)
        {
            string msg = args.Length == 0 ? fmt : string.Format(fmt, args);
            log.LogWarning(msg);
        }

        public void LogError(string fmt, params object[] args)
        {
            string msg = args.Length == 0 ? fmt : string.Format(fmt, args);
            log.LogError(msg);
        }

        public void LogErrorFromException(Exception e)
        {
            log.LogErrorFromException(e);
        }

        public void LogVerbose(string fmt, params object[] args)
        {
            string msg = args.Length == 0 ? fmt : string.Format(fmt, args);
            log.LogMessage(Microsoft.Build.Framework.MessageImportance.Low, msg);
        }
    }
}
