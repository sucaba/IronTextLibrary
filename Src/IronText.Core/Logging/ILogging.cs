
namespace IronText.Framework
{
    public interface ILogging
    {
        int ErrorCount { get; }

        int WarningCount { get; }

        void Reset();

        void WriteTotal();

        void Write(LogEntry entry);
    }
}
