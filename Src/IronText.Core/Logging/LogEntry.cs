using System.Reflection;

namespace IronText.Framework
{
    public class LogEntry
    {
        public Severity Severity = Severity.Message;
        public Loc      Location;
        public HLoc     HLocation;
        public string   Code;
        public string   Message;
        public MemberInfo Member;
    }
}
