using System.Collections.Generic;
using System.IO;
using IronText.Diagnostics;
using IronText.Extensibility;
using IronText.Reflection.Reporting;

namespace IronText.Framework
{
    public class ScannerGraphAttribute : LanguageMetadataAttribute, IReport
    {
        private readonly string fileName;

        public ScannerGraphAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<IReport> GetReports()
        {
            return new [] { this };
        }

        public void Build(IReportData data)
        {
            string scanModeFileName = Path.GetFileName(fileName) + Path.GetExtension(fileName);
            string path = Path.Combine(data.DestinationDirectory, scanModeFileName);
            using (var graph = new GvGraphView(path))
            {
                var dfa = data.GetScannerAutomata();
                dfa.DescribeGraph(graph);
            }
        }
    }
}
