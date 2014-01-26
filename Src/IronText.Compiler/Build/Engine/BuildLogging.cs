using System;
using System.Reflection;
using System.Text;
using IronText.Framework;
using IronText.Logging;

namespace IronText.Build
{
    public sealed class BuildLogging : ILogging
    {
        private int errorCount;
        private int warningCount;

        public void Write(LogEntry entry)
        {
            switch (entry.Severity)
            {
                case Severity.Error: ++errorCount; break;
                case Severity.Warning : ++warningCount; break;
#if !DIAGNOSTICS
                case Severity.Verbose: return;
#endif
            }


            if (entry.Member != null)
            {
                Console.WriteLine(Date + GetSeverityName(entry.Severity) + ": " + FormatMember(entry.Member) + ": " + entry.Message);
            }
            else
            {
                Console.WriteLine(Date + GetSeverityName(entry.Severity) + ": " + entry.Message);
            }
        }

        private string FormatMember(MemberInfo memberInfo)
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

        private string GetSeverityName(Severity severity)
        {
            switch (severity)
            {
                case Severity.Error: return "error";
                case Severity.Warning: return "warning";
                case Severity.Message: return "message";
                case Severity.Verbose: return "verbose";
                default : return "?";
            }
        }

        static string Date 
        { 
            get 
            {
                var now = DateTime.Now;
                return string.Format("[{0:D3}.{1:D3}] ", now.Second, now.Millisecond);
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
            Console.WriteLine(
                "================ {0} errors {1} warnings ================",
                errorCount,
                warningCount);            
        }
    }
}
