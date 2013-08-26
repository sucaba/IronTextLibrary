using System;

namespace IronText.Build.DerivedAssembly
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class MetadataSyntaxAttribute : Attribute
    {
        public Type SyntaxType { get; set; }
    }
}
