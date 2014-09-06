using System.Collections.Generic;
using System.IO;
using System.Text;
using IronText.Extensibility;
using IronText.Reflection.Reporting;

namespace IronText.Reports
{
    public class GrammarReport: IReport
    {
        private readonly string fileName;

        public GrammarReport(string fileName)
        {
            this.fileName = fileName;
        }

        public void Build(IReportData data)
        {
            string path = Path.Combine(data.DestinationDirectory, fileName);

            using (var grammarFile = new StreamWriter(path, false, Encoding.UTF8))
            {
                grammarFile.Write(data.Grammar);
            }
        }
    }
}
