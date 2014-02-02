using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronText.Diagnostics;
using IronText.Extensibility;
using IronText.Reflection.Managed;
using IronText.Reporting;

namespace IronText.Framework
{
    public class ScannerGraphAttribute : LanguageMetadataAttribute
    {
        private readonly string fileName;

        public ScannerGraphAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<ReportBuilder> GetReportBuilders()
        {
            yield return WriteGvGraph;
        }

        private void WriteGvGraph(IReportData data)
        {
            foreach (var condition in data.Grammar.Conditions)
            {
                int i = condition.Index;
                string scanModeFileName = Path.GetFileName(fileName) + "_" + i + Path.GetExtension(fileName);
                string path = Path.Combine(data.DestinationDirectory, scanModeFileName);
                using (var graph = new GvGraphView(path))
                {
                    var dfa = condition.Joint.The<IScannerAutomata>();
                    dfa.DescribeGraph(graph);
                }
            }
        }
    }
}
