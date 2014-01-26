using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple=false)]
    public class DemandAttribute : LanguageMetadataAttribute
    {
        public DemandAttribute() : this(true) { }

        public DemandAttribute(bool value) { this.Value = value; }

        public bool Value { get; private set; }

        public override IEnumerable<ICilMetadata> GetChildren()
        {
            var type = (Type)Member;
            var result = new List<ICilMetadata>(EnumerateDirectChildren());
            result.AddRange(MetadataParser.GetTypeMetaChildren(Parent ?? this, type));

            return result.Where(child => !(child is ScanBaseAttribute));
        }

        public override IEnumerable<CilSymbolFeature<CilContextProvider>> GetTokenContextProvider(ITokenPool tokenPool)
        {
            if (Parent == null)
            {
                var type = (Type)Member;
                return new[]
                { 
                    new CilSymbolFeature<CilContextProvider>(
                            tokenPool.GetToken(type),
                            new CilContextProvider(type))
                };
            }

            return new CilSymbolFeature<CilContextProvider>[0];
        }

        private IEnumerable<ICilMetadata> EnumerateDirectChildren()
        {
            var result = MetadataParser
                .EnumerateAndBind(Member)
                .Where(m => !(m is DemandAttribute))
                ;

            return result;
        }
    }
}
