using IronText.Extensibility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SubContextAttribute : LanguageMetadataAttribute
    {
        public override IEnumerable<ICilMetadata> GetChildren()
        {
            return MetadataParser.EnumerateAndBind(((PropertyInfo)Member).PropertyType);
        }
    }
}
