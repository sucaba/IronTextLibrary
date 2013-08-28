
namespace IronText.Framework
{
    sealed class ExceptionLogging : ILogging
    {
        public readonly static ExceptionLogging Instance = new ExceptionLogging();

        public void Write(LogEntry entry)
        {
            throw new SyntaxException(entry.Location, entry.HLocation, entry.Message);
        }

        public int ErrorCount { get { return 0; } }

        public int WarningCount { get { return 0; } }

        public void Reset() { }

        public void WriteTotal() { }
    }
}
