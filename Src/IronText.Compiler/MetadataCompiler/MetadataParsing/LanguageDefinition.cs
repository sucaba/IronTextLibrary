﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    internal class LanguageDefinition : ITokenPool
    {
        private readonly List<ICilMetadata>              allMetadata;
        private readonly List<CilProductionDef>          allParseRules;
        private readonly List<CilScanCondition>          allScanConditions;
        private readonly List<SymbolFeature<Precedence>> precedence;

        private readonly CilMergerDef[] allMergeRules;

        public LanguageDefinition(Type startType, ILogging logging)
        {
            this.IsValid = true;

            ITokenPool tokenPool = this;

            var startMeta = MetadataParser.EnumerateAndBind(startType);
            if (startMeta.Count() == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "No metadata found in language definition '{0}'",
                        startType.FullName));
            }

            this.TokenRefResolver = new CilSymbolRefResolver();

            this.Start = tokenPool.AugmentedStart;


            var collector = new MetadataCollector(this, logging);
            collector.AddToken(Start);
            foreach (var meta in startMeta)
            {
                collector.AddMeta(meta);
            }

            if (collector.HasInvalidData)
            {
                this.IsValid = false;
            }

            this.allMetadata = collector.AllMetadata;
            this.allParseRules = collector.AllParseRules;

            foreach (var tid in collector.AllTokens)
            {
                TokenRefResolver.Link(tid);
            }

            var categories = new [] { SymbolCategory.Beacon, SymbolCategory.DoNotInsert, SymbolCategory.DoNotDelete, SymbolCategory.ExplicitlyUsed };

            foreach (var category in categories)
            {
                foreach (var tokenRef in allMetadata.SelectMany(m => m.GetTokensInCategory(this, category)))
                {
                    var def = TokenRefResolver.Resolve(tokenRef);
                    def.Categories |= category; 
                }
            }

            if (allParseRules.Count == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Language definition '{0}' should have at least one parse rule",
                        startType.FullName));
            }

            this.allMergeRules 
                = allMetadata
                    .SelectMany(meta => meta.GetMergeRules(collector.AllTokens, this))
                    .ToArray();

            var terminals = collector.AllTokens.Except(allParseRules.Select(r => r.Left).Distinct()).ToArray();

            var scanDataCollector = new ScanDataCollector(terminals, this, logging);
            scanDataCollector.AddScanMode(startType);
            if (scanDataCollector.HasInvalidData)
            {
                this.IsValid = false;
            }

            CheckAllScanRulesDefined(scanDataCollector.UndefinedTerminals, startType, logging);

            allScanConditions = scanDataCollector.ScanConditions;
            LinkRelatedTokens(allScanConditions);

            precedence          = allMetadata.SelectMany(m => m.GetTokenPrecedence(tokenPool)).ToList();

            ContextProviders    = allMetadata.SelectMany((m, index) => m.GetTokenContextProvider(tokenPool)).ToList();

            this.ReportBuilders = allMetadata.SelectMany(m => m.GetReportBuilders()).ToArray();
        }

        private void CheckAllScanRulesDefined(List<CilSymbolRef> undefinedTerminals, Type member, ILogging logging)
        {
            if (undefinedTerminals.Count == 0)
            {
                return;
            }

            var message = new StringBuilder("Undefined scan or parse rules for tokens: ");
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

                message.Append(term.TokenType.Name);
            }

            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Warning,
                    Message  = message.ToString(),
                    Member   = member,
                });
        }

        public bool IsValid { get; private set; }

        private void LinkRelatedTokens(List<CilScanCondition> allScanModes)
        {
            foreach (var scanMode in allScanModes)
            {
                foreach (var scanRule in scanMode.ScanRules)
                {
                    foreach (CilSymbolRef symbolConstraint in scanRule.AllOutcomes)
                    {
                        if (TokenRefResolver.Contains(symbolConstraint))
                        {
                            TokenRefResolver.Link(symbolConstraint);
                        }
                    }
                }
            }
        }

        public CilSymbolRef Start { get; set; }

        public ReportBuilder[] ReportBuilders { get; private set; }

        public ITokenRefResolver TokenRefResolver { get; private set; }

        public IEnumerable<SymbolFeature<Precedence>> Precedence { get { return precedence; } }

        public IList<CilProductionDef> ProductionDefs { get { return allParseRules; } }

        public IList<CilMergerDef> MergerDefs { get { return allMergeRules; } }

        public IList<CilScanCondition> ScanModes { get { return allScanConditions; } }

        public IList<SymbolFeature<CilContextProvider>> ContextProviders { get; private set; }

        CilSymbolRef ITokenPool.AugmentedStart
        {
            get { return CilSymbolRef.Typed(typeof(void)); }
        }

        CilSymbolRef ITokenPool.ScanSkipToken
        {
            get { return CilSymbolRef.Typed(typeof(void)); }
        }

        CilSymbolRef ITokenPool.GetToken(Type tokenType)
        {
            return CilSymbolRef.Typed(tokenType);
        }

        CilSymbolRef ITokenPool.GetLiteral(string keyword)
        {
            return CilSymbolRef.Literal(keyword);
        }

        private static IEnumerable<T> EnumerableMutable<T>(IList<T> list)
        {
            for (int i = 0; i != list.Count; ++i)
            {
                yield return list[i];
            }
        }
    }
}
