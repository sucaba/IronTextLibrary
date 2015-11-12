using IronText.Extensibility;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using System.Text.RegularExpressions;

namespace IronText.Framework
{
    public class LiteralAttribute : ScanBaseAttribute
    {
        // TODO: Logic should not be present in attribute constructor
        public LiteralAttribute(string text, Disambiguation disambiguation = Disambiguation.Exclusive)
        {
            base.LiteralText = text;
            base.Disambiguation = disambiguation;

            // Look-ahead is needed because .net Regex does not select longest match but the first alternative.
            this.RegexPattern = Regex.Escape(text);
            if (text != "(" && text != ")")
            {
                this.RegexPattern += @"(?=[ \t\(\)"";]|$)";
            }

            this.Pattern = ScannerUtils.Escape(text);
        }

        public override bool Validate(ILogging logging)
        {
            if (string.IsNullOrEmpty(LiteralText))
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = "Literal cannot be null or empty string.",
                        Origin = ReflectionUtils.ToString(this.Member)
                    });
                return false;
            }

            return base.Validate(logging);
        }
    }
}
