using IronText.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface,AllowMultiple=false)]
    public sealed class VocabularyAttribute : LanguageMetadataAttribute
    {
        public override IEnumerable<ICilMetadata> GetChildren()
        {
            var result = EnumerateDirectChildren()
                        .Concat(MetadataParser.GetTypeMetaChildren(Parent ?? this, (Type)Member))
                        .ToArray();

            return result;
        }

        private IEnumerable<ICilMetadata> EnumerateDirectChildren()
        {
            var result = MetadataParser
                .EnumerateAndBind(Member)
                .Where(m => !(m is VocabularyAttribute))
                .ToArray();

            return result;
        }

    }
}
