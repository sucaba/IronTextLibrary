using System.Collections.Generic;
using System.IO;
using IronText.Extensibility;
using IronText.Reflection.Reporting;

namespace IronText.Framework
{
    public class ParserGraphAttribute : LanguageMetadataAttribute, IReport
    {
        private readonly string fileName;

        public ParserGraphAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<IReport> GetReports()
        {
            return new[] { this };
        }

        public void Build(IReportData data)
        {
            string path = Path.Combine(data.DestinationDirectory, fileName);

            var graph = new LrGraph(data);
            graph.WriteGv(path);
        }
    }
}
