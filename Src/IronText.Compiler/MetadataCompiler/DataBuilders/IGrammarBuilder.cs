using System.Collections.Generic;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reporting;

namespace IronText.MetadataCompiler
{
    public interface IGrammarBuilder
    {
        IEnumerable<ReportBuilder> ReportBuilders { get; }

        Grammar Build(IGrammarSource source, ILogging logging);
    }
}
