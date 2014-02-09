using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Logging;
using IronText.Reflection.Reporting;

namespace IronText.Reflection.Managed
{
    internal class CilGrammar
    {
        private readonly List<ICilMetadata>                 metadata;
        private readonly List<CilProduction>                productions;
        private readonly List<CilCondition>                 conditions;
        private readonly CilMerger[]                        mergers;
        private readonly List<CilSymbolFeature<Precedence>> precedence;

        public CilGrammar(CilGrammarSource languageName, ILogging logging)
        {
            Type definitionType = languageName.DefinitionType;

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
            scanDataCollector.AddCondition(definitionType);
            if (scanDataCollector.HasInvalidData)
            {
                this.IsValid = false;
            }

            conditions = scanDataCollector.Conditions;
            LinkRelatedTokens(conditions);

            var allTerms = (from t in scanDataCollector.Terminals
                           let def = SymbolResolver.Resolve(t)
                           where def != null
                           select def)
                           .Distinct();
            var termsProducedByScanner =
                            (from cond in scanDataCollector.Conditions
                             from prod in cond.Matchers
                             from outcome in prod.AllOutcomes
                             let def = SymbolResolver.Resolve(outcome)
                             where def != null
                             select def)
                            .Distinct();
            var undefinedTerminals = allTerms
                .Where(symbol => !IsSpecialSymbol(symbol))
                .Except(termsProducedByScanner);

            CheckAllScanRulesDefined(undefinedTerminals, languageName.Origin, logging);

            precedence = metadata.SelectMany(m => m.GetSymbolPrecedence()).ToList();

            ContextProviders = metadata.SelectMany((m, index) => m.GetSymbolContextProviders());

            this.Reports = metadata.SelectMany(m => m.GetReports()).ToArray();
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

        public bool IsValid { get; private set; }

        private void LinkRelatedTokens(List<CilCondition> conditions)
        {
            foreach (var condition in conditions)
            {
                foreach (var scanProd in condition.Matchers)
                {
                    foreach (CilSymbolRef symbol in scanProd.AllOutcomes)
                    {
                        if (SymbolResolver.Contains(symbol))
                        {
                            SymbolResolver.Link(symbol);
                        }
                    }
                }
            }
        }

        public CilSymbolRef            Start          { get; private set; }

        public IReport[]               Reports        { get; private set; }

        public ICilSymbolResolver      SymbolResolver { get; private set; }

        public IList<CilProduction>    Productions    { get { return productions; } }

        public IList<CilMerger>        Mergers        { get { return mergers; } }

        public IList<CilCondition> ScanConditions { get { return conditions; } }

        public IEnumerable<CilSymbolFeature<Precedence>> Precedence { get { return precedence; } }

        public IEnumerable<CilSymbolFeature<CilContextProvider>> ContextProviders { get; private set; }

        private static bool IsSpecialSymbol(CilSymbol symbol)
        {
            return symbol.Type == typeof(Exception);
        }
    }
}
