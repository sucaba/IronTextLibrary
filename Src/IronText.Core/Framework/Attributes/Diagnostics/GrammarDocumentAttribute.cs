using IronText.Reports;

namespace IronText.Framework
{
    public class GrammarDocumentAttribute : ReportAttribute
    {
        public GrammarDocumentAttribute(string fileName)
            : base(typeof(GrammarReport), fileName)
        {
        }
    }
}
