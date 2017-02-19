using System;

namespace IronText.DI
{
    public interface IDynamicDependency
    {
        Type Implementation { get; }
    }

    public interface IDynamicDependency<TContract> : IDynamicDependency
    {
    }
}
