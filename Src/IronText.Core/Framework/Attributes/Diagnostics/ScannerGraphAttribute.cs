using System.Collections.Generic;
using System.IO;
using IronText.Diagnostics;
using IronText.Extensibility;

namespace IronText.Framework
{
    public class ScannerGraphAttribute : LanguageMetadataAttribute
    {
        private readonly string fileName;

        public ScannerGraphAttribute(string fileName)
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

            var modeType = data.ScanModes[0].ScanModeType;
            var dfa = data.GetScanModeDfa(modeType);
            using (var graph = new GvGraphView(path))
            {
                dfa.DescribeGraph(graph);
            }
        }
    }
}
