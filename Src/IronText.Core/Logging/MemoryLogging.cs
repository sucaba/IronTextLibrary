using System.Collections.Generic;

namespace IronText.Logging
{
    class MemoryLogging : ILogging
    {
        private int errorCount;
        private int warningCount;

        public MemoryLogging()
        {
            this.Entries = new List<LogEntry>();
        }

        public List<LogEntry> Entries { get; private set; }

        public void Write(LogEntry entry)
        {
            switch (entry.Severity)
            {
                case Severity.Error: ++errorCount; break;
                case Severity.Warning: ++warningCount; break;
            }

            Entries.Add(entry);
        }

        public int ErrorCount { get { return errorCount; } }

        public int WarningCount { get { return warningCount; } }

        public void Reset()
        {
            errorCount = 0;
            warningCount = 0;
            Entries.Clear();
        }

        public void WriteTotal()
        {
        }
    }
}
