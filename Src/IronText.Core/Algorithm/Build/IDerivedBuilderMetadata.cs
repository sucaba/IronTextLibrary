using System;

namespace IronText.Build
{
    public interface IDerivedBuilderMetadata
    {
        Type BuilderType { get; }

        bool IsIncludedInBuild(Type declaringType);
    }
}
