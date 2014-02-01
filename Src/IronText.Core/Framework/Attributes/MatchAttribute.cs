using IronText.Logging;
using IronText.Reflection;

namespace IronText.Framework
{
    public class MatchAttribute : ScanBaseAttribute
    {
        public MatchAttribute() { }

        public MatchAttribute(string sre, Disambiguation disambiguation = Disambiguation.Contextual)
        {
            Pattern = sre;
            Disambiguation = disambiguation;
        }

        // Allow providing regular expression pattern for bootstrapping
        internal MatchAttribute(string scanPattern, string rePattern, Disambiguation disambiguation = Disambiguation.Contextual)
        {
            Pattern = scanPattern;
            RegexPattern = rePattern;
            Disambiguation = disambiguation;
        }

        public override bool Validate(ILogging logging)
        {
            if (string.IsNullOrEmpty(Pattern))
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = "Scan pattern cannot be null or empty string.",
                        Member = this.Member
                    });

                return false;
            }

            return base.Validate(logging);
        }
    }
}
