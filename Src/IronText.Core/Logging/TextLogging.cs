using System;
using System.IO;

namespace IronText.Logging
{
    sealed class TextLogging : ILogging
    {
        private readonly TextWriter output;

        public TextLogging(TextWriter output)
        {
            this.output = output;
        }

        public void Write(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            switch (entry.Severity)
            {
                case Severity.Error:   ++ErrorCount;   break;
                case Severity.Warning: ++WarningCount; break;
            }

            output.WriteLine(entry.ToCanonicFormat());
        }

        public int ErrorCount   { get; private set; }

        public int WarningCount { get; private set; }

        public void Reset()
        {
            ErrorCount = 0;
            WarningCount = 0;
        }

        public void WriteTotal()
        {
            output.WriteLine(
                "================ {0} errors {1} warnings ================",
                ErrorCount,
                WarningCount);            
        }
    }
}
