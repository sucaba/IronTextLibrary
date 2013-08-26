using System;

namespace IronText.Build
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
    public class DerivedAssemblyMarker : Attribute
    {
    }
}
