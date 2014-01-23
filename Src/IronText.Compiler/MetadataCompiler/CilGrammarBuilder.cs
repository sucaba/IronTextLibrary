using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Framework.Reflection;

namespace IronText.MetadataCompiler
{
    public class CilGrammarBuilder
    {
        private readonly ILogging logging;

        public CilGrammarBuilder(ILogging logging)
        {
            this.logging = logging;
            this.ReportBuilders = new List<ReportBuilder>();
        }

        public List<ReportBuilder> ReportBuilders { get; private set; }

        public EbnfGrammar Build(LanguageName languageName)
        {
            var grammar = new EbnfGrammar();
            return grammar;
        }
    }
}
