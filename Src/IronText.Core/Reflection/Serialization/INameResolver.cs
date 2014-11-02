using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal interface INameResolver<T>
    {
        T Resolve(string name, bool createMissing = false);
    }
}
