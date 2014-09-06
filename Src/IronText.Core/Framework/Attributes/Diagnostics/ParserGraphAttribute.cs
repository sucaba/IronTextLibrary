using IronText.Reports;

namespace IronText.Framework
{
    public class ParserGraphAttribute : ReportAttribute
    {
        public ParserGraphAttribute(string fileName)
            : base(typeof(ParserGraphReport), fileName)
        {
        }
    }
}
