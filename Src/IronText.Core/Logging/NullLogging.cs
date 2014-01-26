namespace IronText.Logging
{
    sealed class NullLogging : ILogging
    {
        public static readonly NullLogging Instance = new NullLogging();

        public void Write(LogEntry entry) { }

        public int ErrorCount { get { return 0; } }

        public int WarningCount { get { return 0; } }

        public void Reset() { }

        public void WriteTotal() { }
    }
}
