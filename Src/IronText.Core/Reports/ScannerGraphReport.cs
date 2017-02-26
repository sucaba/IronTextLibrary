using IronText.Diagnostics;
using IronText.Reflection.Reporting;
using IronText.Reflection.Reporting.Rendering;
using System.IO;

namespace IronText.Reports
{
    public class ScannerGraphReport : IReport
    {
        private readonly string fileName;

        public ScannerGraphReport(string fileName)
        {
            this.fileName = fileName;
        }

        public void Build(IReportData data)
        {
            string scanModeFileName = Path.GetFileName(fileName) + Path.GetExtension(fileName);
            string path = Path.Combine(data.DestinationDirectory, scanModeFileName);
            using (var graph = new GvGraphView(path))
            {
                var renderer = new ScannerAutomataGraphRenderer(graph);
                renderer.DescribeGraph(data.GetScannerAutomata());
            }
        }
    }
}
