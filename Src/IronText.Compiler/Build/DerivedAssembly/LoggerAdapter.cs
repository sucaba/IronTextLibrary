using System;
using IronText.Framework;

namespace IronText.Build.DerivedAssembly
{
    class LoggerAdapter : ILogger
    {
        private readonly ILogging logging;

        public LoggerAdapter(ILogging logging)
        {
            this.logging = logging;
        }

        public void LogError(string fmt, params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                     Severity = Severity.Error,
                     Message = string.Format(fmt, args),
                });
        }

        public void LogErrorFromException(Exception e)
        {
            logging.Write(
                new LogEntry
                {
                     Severity = Severity.Error,
                     Message = e.Message,
                });
        }

        public void LogWarning(string fmt, params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                     Severity = Severity.Warning,
                     Message = string.Format(fmt, args),
                });
        }

        public void LogMessage(string fmt, params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                     Severity = Severity.Message,
                     Message = string.Format(fmt, args),
                });
        }

        public void LogVerbose(string fmt, params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                     Severity = Severity.Verbose,
                     Message = string.Format(fmt, args),
                });
        }
    }
}
