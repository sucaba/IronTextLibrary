using IronText.Framework;
using System.Text;
using System;

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
            var msg = MemberContext(entry) + entry.Message;
            switch (entry.Severity)
            {
                case Severity.Error:
                    ++errorCount;
                    logger.LogError(msg);
                    break;
                case Severity.Message:
                    ++warningCount;
                    logger.LogMessage(msg);
                    break;
                case Severity.Verbose:
                    logger.LogVerbose(msg);
                    break;
                case Severity.Warning:
                default:
                    logger.LogWarning(msg);
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

        private static string MemberContext(LogEntry entry)
        {
            if (entry.Member == null)
            {
                return "";
            }

            return FormatMember(entry.Member) + ": ";
        }

        private static string FormatMember(System.Reflection.MemberInfo memberInfo)
        {
            var output = new StringBuilder();
            var asType = memberInfo as Type;
            if (asType != null)
            {
                output.Append(asType.FullName);
            }
            else
            {
                var type = memberInfo.DeclaringType;
                output.Append(type.FullName).Append("::").Append(memberInfo.Name);
            }
            return output.ToString();
        }
    }
}
