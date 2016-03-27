
namespace IronText.Logging
{
    public class LogEntry
    {
        public Severity Severity = Severity.Message;
        public HLoc     HLocation;
        public string   Code;
        public string   Message;
        public string   Origin;
    }
}
