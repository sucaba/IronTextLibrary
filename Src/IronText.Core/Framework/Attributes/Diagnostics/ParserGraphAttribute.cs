using System.Collections.Generic;
using System.IO;
using IronText.Extensibility;

namespace IronText.Framework
{
    public class ParserGraphAttribute : LanguageMetadataAttribute
    {
        private readonly string fileName;

        public ParserGraphAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<ReportBuilder> GetLanguageDataActions()
        {
            yield return WriteGvGraph;
        }

        private void WriteGvGraph(IReportData data)
        {
            string path = Path.Combine(data.GetDestinationDirectory(), fileName);

            var graph = new LrGraph(data);
            graph.WriteGv(path);
        }
    }
}
