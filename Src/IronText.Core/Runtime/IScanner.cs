using System.Collections.Generic;

namespace IronText.Runtime
{
    public interface IScanner 
        : ISequence<Message>
        , IEnumerable<Message>
    {
    }
}
