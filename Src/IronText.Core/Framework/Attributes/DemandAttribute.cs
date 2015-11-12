using IronText.Extensibility;
using IronText.Reflection.Managed;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public override IEnumerable<CilSymbolFeature<CilSemanticScope>> GetLocalContextProviders()
        {
            if (Parent == null)
            {
                var type = (Type)Member;
                return new[]
                { 
                    new CilSymbolFeature<CilSemanticScope>(
                            CilSymbolRef.Create(type),
                            new CilSemanticScope(type))
                };
            }

            return new CilSymbolFeature<CilSemanticScope>[0];
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
