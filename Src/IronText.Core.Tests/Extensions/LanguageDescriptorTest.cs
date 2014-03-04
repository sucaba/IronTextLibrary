using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Logging;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Reflection.Reporting;
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
            var target = new CilGrammar(new CilGrammarSource(typeof(IMetadtaTest0)), logging);
            Assert.AreEqual(CilSymbolRef.Create(typeof(void)), target.Start);
            Assert.AreEqual(2, target.SymbolResolver.Definitions.Count());
            Assert.AreEqual(
                new [] { typeof(void), typeof(int) },
                target.SymbolResolver.Definitions.Select(def => def.Type).ToArray());
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
                    yield return new CilProduction(
                        outcome: CilSymbolRef.Create(typeof(void)),
                        pattern: new[] { CilSymbolRef.Create(typeof(int)) },
                        context: CilContextRef.None,
                        actionBuilder: code => { code.Emit(il => il.Ldnull().Ret()); });
                }
            }

            public IEnumerable<CilMatcher> GetMatchers()
            {
                return Enumerable.Empty<CilMatcher>();
            }

            public IEnumerable<CilSymbolRef> GetSymbolsInCategory(SymbolCategory category)
            {
                return Enumerable.Empty<CilSymbolRef>();
            }

            IEnumerable<CilSymbolFeature<Precedence>> ICilMetadata.GetSymbolPrecedence()
            {
                return Enumerable.Empty<CilSymbolFeature<Precedence>>();
            }

            IEnumerable<CilSymbolFeature<CilContextProvider>> ICilMetadata.GetLocalContextProviders()
            {
                return Enumerable.Empty<CilSymbolFeature<CilContextProvider>>();
            }

            IEnumerable<IReport> ICilMetadata.GetReports()
            {
                return Enumerable.Empty<IReport>();
            }

            public IEnumerable<CilMerger> GetMergers(IEnumerable<CilSymbolRef> leftSides)
            {
                return Enumerable.Empty<CilMerger>();
            }
        }
    }
}
