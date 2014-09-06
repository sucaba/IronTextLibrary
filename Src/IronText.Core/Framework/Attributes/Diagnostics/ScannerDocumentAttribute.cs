using IronText.Reports;

namespace IronText.Framework
{
    public class ScannerDocumentAttribute : ReportAttribute
    {
        public ScannerDocumentAttribute(string fileName)
            : base(typeof(ScannerReport), fileName)
        {
        }
    }
}
