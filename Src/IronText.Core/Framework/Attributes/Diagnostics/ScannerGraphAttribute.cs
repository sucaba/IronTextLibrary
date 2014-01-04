using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronText.Diagnostics;
using IronText.Extensibility;
using IronText.Extensibility.Bindings.Cil;

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
            int conditionCount = data.Grammar.ScanConditions.Count;
            for (int i = 0; i != conditionCount; ++i)
            {
                string scanModeFileName = Path.GetFileName(fileName) + "_" + i + Path.GetExtension(fileName);
                string path = Path.Combine(data.DestinationDirectory, scanModeFileName);

                var condition = data.Grammar.ScanConditions[i];
                var binding = condition.Joint.The<CilScanConditionBinding>();

                var dfa = data.GetScanModeDfa(binding.ConditionType);
                using (var graph = new GvGraphView(path))
                {
                    dfa.DescribeGraph(graph);
                }
            }
        }
    }
}
