using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface,AllowMultiple=false)]
    public sealed class VocabularyAttribute : LanguageMetadataAttribute
    {
        public override IEnumerable<ILanguageMetadata> GetChildren()
        {
            var result = EnumerateDirectChildren()
                        .Concat(MetadataParser.GetTypeMetaChildren(Parent, (Type)Member))
                        .ToArray();

            return result;
        }

        private IEnumerable<ILanguageMetadata> EnumerateDirectChildren()
        {
            var result = MetadataParser
                .EnumerateAndBind(Member)
                .Where(m => !(m is VocabularyAttribute))
                .ToArray();

            return result;
        }

    }
}
