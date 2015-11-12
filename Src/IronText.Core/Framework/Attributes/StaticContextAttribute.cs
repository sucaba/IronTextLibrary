using IronText.Extensibility;
using System;
using System.Collections.Generic;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple = true)]
    public class StaticContextAttribute : LanguageMetadataAttribute
    {
        public StaticContextAttribute(Type moduleType) {  this.ModuleType = moduleType; }

        public Type ModuleType { get; set; }

        public override IEnumerable<ICilMetadata> GetChildren()
        {
            return MetadataParser.EnumerateAndBind(ModuleType);
        }
    }
}
