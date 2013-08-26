using System.Collections.Generic;
using System.IO;
using System.Text;
using IronText.Extensibility;

namespace IronText.Framework
{
    public class GrammarDocumentAttribute : LanguageMetadataAttribute
    {
        private readonly string fileName;

        public GrammarDocumentAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<LanguageDataAction> GetLanguageDataActions()
        {
            yield return WriteGrammarFile;
        }

        private void WriteGrammarFile(LanguageData data)
        {
            string path = Path.Combine(data.GetDestinationDirectory(), fileName);

            using (var grammarFile = new StreamWriter(path, false, Encoding.UTF8))
            {
                grammarFile.Write(data.Grammar);
            }
        }
    }
}
