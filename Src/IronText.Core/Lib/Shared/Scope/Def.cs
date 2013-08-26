using System.Collections.Generic;

namespace IronText.Lib.Shared
{
    /// <summary>
    /// States:
    /// 1. Declared
    /// 2. Defined (with optional name)
    /// </summary>
    /// <typeparam name="TNs"></typeparam>
    public interface Def<TNs>
    {
        string Name { get; set; }

        Ref<TNs> GetRef();

        object Value { get; set; }

        bool IsExplicit { get; set; }

        IEnumerable<Ref<TNs>> RefsBefore { get; }
    }
}
