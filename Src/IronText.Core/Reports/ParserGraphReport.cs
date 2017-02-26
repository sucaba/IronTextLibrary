using IronText.Diagnostics;
using IronText.Reporting;
using IronText.Reporting.Rendering;
using System.IO;

namespace IronText.Reports
{
    public class ParserGraphReport : IReport
    {
        private readonly string fileName;

        public ParserGraphReport(string fileName)
        {
            this.fileName = fileName;
        }

        public void Build(IReportData data)
        {
            string path = Path.Combine(data.DestinationDirectory, fileName);

            using (var graph = new GvGraphView(path))
            {
                var renderer = new ParserAutomataGraphRenderer(graph);
                renderer.Render(data.ParserAutomata);
            }
        }
    }
}
