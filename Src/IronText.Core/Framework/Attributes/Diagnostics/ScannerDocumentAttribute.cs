using System.Collections.Generic;
using System.IO;
using System.Text;
using IronText.Extensibility;

namespace IronText.Framework
{
    public class ScannerDocumentAttribute : LanguageMetadataAttribute
    {
        private readonly string fileName;

        public ScannerDocumentAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<ReportBuilder> GetReportBuilders()
        {
            yield return WriteScannerFile;
        }

        private void WriteScannerFile(IReportData data)
        {
            string path = Path.Combine(data.DestinationDirectory, fileName);

            using (var file = new StreamWriter(path, false, Encoding.UTF8))
            {
                foreach (var scanMode in data.ScanModes)
                {
                    file.WriteLine("-------------------------------------");
                    file.WriteLine("ScanMode {0}:", scanMode.ScanModeType.FullName);
                    foreach (ScanRule scanRule in scanMode.ScanRules)
                    {
                        file.WriteLine(" " + scanRule.ToString());
                    }
                }
            }
        }
    }
}
