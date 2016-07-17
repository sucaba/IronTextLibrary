namespace IronText.Logging
{
    public class LogEntry
    {
        public Severity Severity { get; set; } = Severity.Message;

        public Loc      Location { get; set; }

        public string   Code     { get; set; }

        public string   Message  { get; set; }

        public string   Origin   { get; set; }

        public string SeverityName
        {
            get
            {
                switch (Severity)
                {
                    case Severity.Verbose: return "diagnostics";
                    case Severity.Message: return "message";
                    case Severity.Warning: return "warning";
                    case Severity.Error:   return "error";
                    default: return "";
                }
            }
        }

        public string ToCanonicFormat() =>
            $@"{Location.FilePath}({Location.FirstLine},{Location.FirstColumn}): {SeverityName} {Code}: {Message}";

        public override string ToString() => ToCanonicFormat();
    }
}
