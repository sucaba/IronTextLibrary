using IronText.Reports;

namespace IronText.Framework
{
    public class ScannerGraphAttribute : ReportAttribute
    {
        public ScannerGraphAttribute(string fileName)
            : base(typeof(ScannerGraphReport), fileName)
        {
        }
    }
}
