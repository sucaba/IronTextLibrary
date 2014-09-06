using IronText.Reports;

namespace IronText.Framework
{
    public class DescribeParserStateMachineAttribute : ReportAttribute
    {
        public DescribeParserStateMachineAttribute(string fileName)
            : base(typeof(ParserStateMachineReport), fileName)
        {
        }
    }
}
