using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.MetadataCompiler;
using NUnit.Framework;

namespace IronText.Tests.Extensibility
{
    [TestFixture]
    public class LanguageDescriptorTest
    {
        [Test]
        public void LanguageDescriptorCollectDataFromTypeMetadata()
        {
            ILogging logging = new MemoryLogging();
            var target = new LanguageDefinition(typeof(IMetadtaTest0), logging);
            ITokenPool tokenPool = target;
            Assert.AreEqual(tokenPool.GetToken(typeof(void)), target.Start);
            Assert.AreEqual(2, target.TokenRefResolver.Definitions.Count());
            Assert.AreEqual(
                new [] { typeof(void), typeof(int) },
                target.TokenRefResolver.Definitions.Select(def => def.TokenType).ToArray());
        }

        [SampleAttr]
        interface IMetadtaTest0 { }

        internal class SampleAttr : Attribute, ILanguageMetadata
        {
            public bool Validate(ILogging logging) { return true; }

            public System.Reflection.MemberInfo Member { get; private set; }

            public ILanguageMetadata Parent { get; private set; }

            public void Bind(ILanguageMetadata parent, System.Reflection.MemberInfo member) { }

            public IEnumerable<ILanguageMetadata> GetChildren() { return Enumerable.Empty<ILanguageMetadata>(); }

            public IEnumerable<ParseRule> GetParseRules(IEnumerable<TokenRef> leftSides, ITokenPool moduleBuilder)
            {
                foreach (var leftSide in leftSides.ToArray())
                {
                    yield return new ParseRule
                    (
                        left : moduleBuilder.GetToken(typeof(void)),
                        parts : new[] { moduleBuilder.GetToken(typeof(int)) },
                        actionBuilder: code => { code.Emit(il => il.Ldnull().Ret()); },
                        instanceDeclaringType : typeof(IMetadtaTest0)
                    );
                }
            }

            public IEnumerable<IScanRule> GetScanRules(ITokenPool moduleBuilder)
            {
                return Enumerable.Empty<IScanRule>();
            }

            public IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category)
            {
                return Enumerable.Empty<TokenRef>();
            }

            IEnumerable<TokenFeature<Precedence>> ILanguageMetadata.GetTokenPrecedence(ITokenPool tokenPool)
            {
                return Enumerable.Empty<TokenFeature<Precedence>>();
            }

            IEnumerable<TokenFeature<ContextProvider>> ILanguageMetadata.GetTokenContextProvider(ITokenPool tokenPool)
            {
                return Enumerable.Empty<TokenFeature<ContextProvider>>();
            }

            IEnumerable<ReportBuilder> ILanguageMetadata.GetReportBuilders()
            {
                return Enumerable.Empty<ReportBuilder>();
            }

            public IEnumerable<MergeRule> GetMergeRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool)
            {
                return Enumerable.Empty<MergeRule>();
            }
        }
    }
}
