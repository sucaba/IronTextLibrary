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
            Assert.AreEqual(CilSymbolRef.Typed(typeof(void)), target.Start);
            Assert.AreEqual(2, target.SymbolResolver.Definitions.Count());
            Assert.AreEqual(
                new [] { typeof(void), typeof(int) },
                target.SymbolResolver.Definitions.Select(def => def.SymbolType).ToArray());
        }

        [SampleAttr]
        interface IMetadtaTest0 { }

        internal class SampleAttr : Attribute, ICilMetadata
        {
            public bool Validate(ILogging logging) { return true; }

            public System.Reflection.MemberInfo Member { get; private set; }

            public ICilMetadata Parent { get; private set; }

            public void Bind(ICilMetadata parent, System.Reflection.MemberInfo member) { }

            public IEnumerable<ICilMetadata> GetChildren() { return Enumerable.Empty<ICilMetadata>(); }

            public IEnumerable<CilProduction> GetProductions(IEnumerable<CilSymbolRef> leftSides)
            {
                foreach (var leftSide in leftSides.ToArray())
                {
                    yield return new CilProduction
                    (
                        left : CilSymbolRef.Typed(typeof(void)),
                        parts : new[] { CilSymbolRef.Typed(typeof(int)) },
                        actionBuilder: code => { code.Emit(il => il.Ldnull().Ret()); },
                        instanceDeclaringType : typeof(IMetadtaTest0)
                    );
                }
            }

            public IEnumerable<CilScanProduction> GetScanRules()
            {
                return Enumerable.Empty<CilScanProduction>();
            }

            public IEnumerable<CilSymbolRef> GetTokensInCategory(SymbolCategory category)
            {
                return Enumerable.Empty<CilSymbolRef>();
            }

            IEnumerable<CilSymbolFeature<Precedence>> ICilMetadata.GetTokenPrecedence()
            {
                return Enumerable.Empty<CilSymbolFeature<Precedence>>();
            }

            IEnumerable<CilSymbolFeature<CilContextProvider>> ICilMetadata.GetTokenContextProvider()
            {
                return Enumerable.Empty<CilSymbolFeature<CilContextProvider>>();
            }

            IEnumerable<ReportBuilder> ICilMetadata.GetReportBuilders()
            {
                return Enumerable.Empty<ReportBuilder>();
            }

            public IEnumerable<CilMerger> GetMergeRules(IEnumerable<CilSymbolRef> leftSides)
            {
                return Enumerable.Empty<CilMerger>();
            }
        }
    }
}
