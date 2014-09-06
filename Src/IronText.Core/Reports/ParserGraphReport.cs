using System.Collections.Generic;
using System.IO;
using IronText.Extensibility;
using IronText.Reflection.Reporting;

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

            var graph = new LrGraph(data);
            graph.WriteGv(path);
        }
    }
}
