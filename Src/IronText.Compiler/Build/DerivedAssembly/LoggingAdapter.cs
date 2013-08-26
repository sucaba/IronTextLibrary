using IronText.Logging;

namespace IronText.Build
{
    class LoggingAdapter : ILogging
    {
        private readonly ILogger logger;
        private int errorCount;
        private int warningCount;

        public LoggingAdapter(ILogger logger)
        {
            this.logger = logger;
        }

        public void Write(LogEntry entry)
        {
            switch (entry.Severity)
            {
                case Severity.Error:
                    ++errorCount;
                    logger.LogError(entry.Message);
                    break;
                case Severity.Message:
                    ++warningCount;
                    logger.LogMessage(entry.Message);
                    break;
                case Severity.Verbose:
                    logger.LogVerbose(entry.Message);
                    break;
                case Severity.Warning:
                default:
                    logger.LogWarning(entry.Message);
                    break;
            }
        }

        public int ErrorCount
        {
            get { return errorCount; }
        }

        public int WarningCount
        {
            get { return warningCount; }
        }

        public void Reset()
        {
            errorCount = 0;
            warningCount = 0;
        }

        public void WriteTotal()
        {
        }
    }
}
