using System.Collections.Generic;
using System.IO;
using System.Text;
using IronText.Extensibility;
using IronText.Reporting;

namespace IronText.Framework
{
    public class GrammarDocumentAttribute : LanguageMetadataAttribute
    {
        private readonly string fileName;

        public GrammarDocumentAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<ReportBuilder> GetReportBuilders()
        {
            yield return WriteGrammarFile;
        }

        private void WriteGrammarFile(IReportData data)
        {
            string path = Path.Combine(data.DestinationDirectory, fileName);

            using (var grammarFile = new StreamWriter(path, false, Encoding.UTF8))
            {
                grammarFile.Write(data.Grammar);
            }
        }
    }
}
