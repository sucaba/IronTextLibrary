using System.Collections.Generic;
using IronText.Framework;

namespace IronText.Runtime
{
    public interface IScanner 
        : ISequence<Msg>
        , IEnumerable<Msg>
    {
    }
}
