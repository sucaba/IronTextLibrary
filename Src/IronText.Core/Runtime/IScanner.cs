using System.Collections.Generic;
using IronText.Framework;

namespace IronText.Framework
{
    public interface IScanner 
        : ISequence<Msg>
        , IEnumerable<Msg>
    {
    }
}
