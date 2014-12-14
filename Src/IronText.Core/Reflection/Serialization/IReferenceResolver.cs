using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal interface IReferenceResolver<T, TRef>
    {
        T Find(TRef reference);

        T Create(TRef reference);

        T Resolve(TRef reference);
    }
}
