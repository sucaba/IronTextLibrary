using System.Collections.Generic;

namespace IronText.Framework
{
    public interface IScanner 
        : ISequence<Msg>
        , IEnumerable<Msg>
    {
    }
}
