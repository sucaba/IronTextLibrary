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
            foreach (var condition in data.Grammar.Conditions)
            {
                int i = condition.Index;
                string scanModeFileName = Path.GetFileName(fileName) + "_" + i + Path.GetExtension(fileName);
                string path = Path.Combine(data.DestinationDirectory, scanModeFileName);
                using (var graph = new GvGraphView(path))
                {
                    var dfa = data.GetScannerAutomata(condition);
                    dfa.DescribeGraph(graph);
                }
            }
        }
    }
}
