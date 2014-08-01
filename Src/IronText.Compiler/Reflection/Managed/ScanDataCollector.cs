using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Logging;
using IronText.Misc;

namespace IronText.Reflection.Managed
{
    class ScanDataCollector : IScanDataCollector
    {
        private readonly List<ICilMetadata> validMetadata;
        private readonly List<ICilMetadata> invalidMetadata;
        private readonly List<CilMatcher>   matchers;
        private readonly List<CilSymbolRef> terminals;
        private int implicitLiteralCount = 0;

        private readonly ILogging logging;

        public ScanDataCollector(IEnumerable<CilSymbolRef> terminals, ILogging logging)
        {
            this.logging = logging;
            this.validMetadata   = new List<ICilMetadata>();
            this.invalidMetadata = new List<ICilMetadata>();
            this.matchers        = new List<CilMatcher>();
            this.terminals       = new List<CilSymbolRef>(terminals);
        }

        public List<CilSymbolRef> Terminals { get { return terminals; } }

        public bool HasInvalidData { get { return invalidMetadata.Count != 0; } }

        public List<CilMatcher> Matchers { get { return matchers; } }

        public void AddMeta(ICilMetadata meta)
        {
            if (validMetadata.Contains(meta, PropertyComparer<ICilMetadata>.Default)
                ||
                invalidMetadata.Contains(meta, PropertyComparer<ICilMetadata>.Default))
            {
                return;
            }

            if (!meta.Validate(logging))
            {
                invalidMetadata.Add(meta);
                return;
            }

            validMetadata.Add(meta);

            foreach (var matcher in meta.GetMatchers())
            {
                AddMatcher(matcher);
            }

            foreach (var childMeta in meta.GetChildren())
            {
                this.AddMeta(childMeta);
            }
        }

        public void AddMatcher(CilMatcher matcher)
        {
            matchers.Add(matcher);
        }

        public void CollectFrom(Type definitionType)
        {
            foreach (var meta in MetadataParser.EnumerateAndBind(definitionType))
            {
                AddMeta(meta);
            }

            var implicitLiterals = 
                (from t in terminals
                 where t.HasLiteral
                 select t.Literal)
                .Except(
                    from matcher in Matchers
                    where matcher.Pattern.IsLiteral
                    select matcher.Pattern.Literal)
                    .ToArray();

            foreach (var literal in implicitLiterals)
            {
                AddImplicitLiteralMatcher(literal);
            }
        }

        internal CilMatcher AddImplicitLiteralMatcher(string literal)
        {
            var result = CreateImplicitLiteralMatcher(literal);
            matchers.Insert(implicitLiteralCount++, result);
            return result;
        }

        private static CilMatcher CreateImplicitLiteralMatcher(string literal)
        {
            var outcome = CilSymbolRef.Create(literal);

            // Generate implicit scan rule for the keyword
            var result  = new CilMatcher
            {
                Context         = CilSemanticRef.None,
                AllOutcomes     = { outcome },
                Disambiguation  = Disambiguation.Exclusive,
                Pattern         = ScanPattern.CreateLiteral(literal),
                ActionBuilder   = code => code
                                    .Emit(il => il.Ldnull())
                                    .ReturnFromAction()
            };

            return result;
        }
    }
}
