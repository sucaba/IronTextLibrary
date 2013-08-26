using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Extensibility;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SubContextAttribute : LanguageMetadataAttribute
    {
        public override IEnumerable<ILanguageMetadata> GetChildren()
        {
            return MetadataParser.EnumerateAndBind(((PropertyInfo)Member).PropertyType);
        }

        public override IEnumerable<Type> GetContextTypes()
        {
            yield return ((PropertyInfo)Member).PropertyType;
        }
    }
}
