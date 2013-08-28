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
            ILogging logging = ExceptionLogging.Instance;
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
                    yield return new ParseRule(this)
                    {
                        Left = moduleBuilder.GetToken(typeof(void)),
                        Parts = new[] { moduleBuilder.GetToken(typeof(int)) },
                        ActionBuilder = code => { code.Emit(il => il.Ldnull().Ret()); }
                    };
                }
            }

            public IEnumerable<ScanRule> GetScanRules(ITokenPool moduleBuilder)
            {
                return Enumerable.Empty<ScanRule>();
            }

            public IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, TokenCategory category)
            {
                return Enumerable.Empty<TokenRef>();
            }

            public IEnumerable<Type> GetContextTypes()
            {
                return Enumerable.Empty<Type>();
            }

            public IEnumerable<SwitchRule> GetSwitchRules(IEnumerable<TokenRef> token, ITokenPool tokenPool)
            {
                return Enumerable.Empty<SwitchRule>();
            }

            IEnumerable<KeyValuePair<TokenRef,Precedence>> ILanguageMetadata.GetTokenPrecedence(ITokenPool tokenPool)
            {
                return Enumerable.Empty<KeyValuePair<TokenRef,Precedence>>();
            }

            IEnumerable<ReportBuilder> ILanguageMetadata.GetLanguageDataActions()
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
