using System;
using System.IO;

namespace IronText.Framework
{
    sealed class TextLogging : ILogging
    {
        private readonly TextWriter output;
        private int errorCount;
        private int warningCount;

        public TextLogging(TextWriter output)
        {
            this.output = output;
        }

        public void Write(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            switch (entry.Severity)
            {
                case Severity.Error: ++errorCount; break;
                case Severity.Warning: ++warningCount; break;
            }

            output.WriteLine(
                "{0}({1},{2}): {3} {4}: {5}",
                entry.Location.FilePath,
                entry.HLocation.FirstLine,
                entry.HLocation.FirstColumn,
                GetSeverityName(entry.Severity),
                entry.Code,
                entry.Message);
        }

        private object GetSeverityName(Severity severity)
        {
            switch (severity)
            {
                case Severity.Verbose:       return "diagnostics";
                case Severity.Message:       return "message";
                case Severity.Warning:       return "warning";
                case Severity.Error:         return "error";
                default: return "";
            }
        }

        public int ErrorCount { get { return errorCount; } }

        public int WarningCount { get { return warningCount; } }

        public void Reset()
        {
            errorCount = 0;
            warningCount = 0;
        }

        public void WriteTotal()
        {
            output.WriteLine(
                "================ {0} errors {1} warnings ================",
                errorCount,
                warningCount);            
        }
    }
}
