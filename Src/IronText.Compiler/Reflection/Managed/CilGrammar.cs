using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Logging;
using IronText.Reflection.Reporting;
using System.Collections.ObjectModel;
using IronText.Runtime;

namespace IronText.Reflection.Managed
{
    class CilGrammar
    {
        private readonly List<ICilMetadata>                 metadata;
        private readonly List<CilProduction>                productions;
        private readonly CilMerger[]                        mergers;
        private readonly List<CilSymbolFeature<Precedence>> precedence;

        public CilGrammar(TypedLanguageSource source, ILogging logging)
        {
            Type definitionType = source.DefinitionType;

            Globals = new CilSemanticScope(definitionType);

            this.IsValid = true;

            var startMeta = MetadataParser.EnumerateAndBind(definitionType);
            if (startMeta.Count() == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "No metadata found in language definition '{0}'",
                        definitionType.FullName));
            }

            this.SymbolResolver = new CilSymbolRefResolver();

            this.Start = CilSymbolRef.Create(typeof(void));

            var collector = new MetadataCollector(logging);
            collector.AddSymbol(Start);
            foreach (var meta in startMeta)
            {
                collector.AddMeta(meta);
            }

            if (collector.HasInvalidData)
            {
                this.IsValid = false;
            }

            this.metadata = collector.Metadata;
            this.productions = collector.Productions;

            foreach (var tid in collector.Symbols)
            {
                SymbolResolver.Link(tid);
            }

            var categories = new [] { SymbolCategory.Beacon, SymbolCategory.DoNotInsert, SymbolCategory.DoNotDelete, SymbolCategory.ExplicitlyUsed };

            foreach (var category in categories)
            {
                foreach (var symbol in metadata.SelectMany(m => m.GetSymbolsInCategory(category)))
                {
                    var def = SymbolResolver.Resolve(symbol);
                    def.Categories |= category; 
                }
            }

            if (productions.Count == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Language definition '{0}' should have at least one parse production",
                        definitionType.FullName));
            }

            this.mergers 
                = metadata
                    .SelectMany(meta => meta.GetMergers(collector.Symbols))
                    .ToArray();

            var terminals = collector.Symbols.Except(productions.Select(r => r.Outcome).Distinct()).ToArray();

            var scanDataCollector = new ScanDataCollector(terminals, logging);
            scanDataCollector.CollectFrom(definitionType);
            if (scanDataCollector.HasInvalidData)
            {
                this.IsValid = false;
            }

            this.Matchers = scanDataCollector.Matchers.AsReadOnly();
            LinkRelatedTokens(this.Matchers);

            var allTerms = (from t in scanDataCollector.Terminals
                           let def = SymbolResolver.Resolve(t)
                           where def != null
                           select def)
                           .Distinct();
            var termsProducedByScanner =
                            (from prod in scanDataCollector.Matchers
                             from outcome in prod.AllOutcomes
                             let def = SymbolResolver.Resolve(outcome)
                             where def != null
                             select def)
                            .Distinct();
            var undefinedTerminals = allTerms
                .Where(symbol => !IsSpecialSymbol(symbol))
                .Except(termsProducedByScanner);

            CheckAllScanRulesDefined(undefinedTerminals, source.GrammarOrigin, logging);

            precedence = metadata.SelectMany(m => m.GetSymbolPrecedence()).ToList();

            LocalSemanticScopes = metadata.SelectMany((m, index) => m.GetLocalContextProviders());

            this.Reports = metadata.SelectMany(m => m.GetReports()).ToArray();
        }

        public bool IsValid { get; private set; }

        public CilSemanticScope        Globals { get; private set; }

        public CilSymbolRef            Start          { get; private set; }

        public IReport[]               Reports        { get; private set; }

        public ICilSymbolResolver      SymbolResolver { get; private set; }

        public IList<CilProduction>    Productions    { get { return productions; } }

        public IList<CilMerger>        Mergers        { get { return mergers; } }

        public ReadOnlyCollection<CilMatcher> Matchers { get; private set; }

        public IEnumerable<CilSymbolFeature<Precedence>> Precedence { get { return precedence; } }

        public IEnumerable<CilSymbolFeature<CilSemanticScope>> LocalSemanticScopes { get; private set; }

        private static bool IsSpecialSymbol(CilSymbol symbol)
        {
            return symbol.Type == typeof(Exception);
        }

        private void LinkRelatedTokens(IEnumerable<CilMatcher> matchers)
        {
            foreach (var matcher in matchers)
            {
                foreach (CilSymbolRef symbol in matcher.AllOutcomes)
                {
                    if (SymbolResolver.Contains(symbol))
                    {
                        SymbolResolver.Link(symbol);
                    }
                }
            }
        }

        private void CheckAllScanRulesDefined(
            IEnumerable<CilSymbol> undefinedTerminals,
            string                 origin,
            ILogging               logging)
        {
            if (undefinedTerminals.Count() == 0)
            {
                return;
            }

            var message = new StringBuilder("Undefined scan or parse productions for tokens: ");
            bool first = true;
            foreach (var term in undefinedTerminals)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    message.Append(", ");
                }

                message.Append(term.ToString());
            }

            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Warning,
                    Message  = message.ToString(),
                    Origin   = origin,
                });
        }
    }
}
