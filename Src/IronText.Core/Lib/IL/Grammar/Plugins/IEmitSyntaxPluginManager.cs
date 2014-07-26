using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Lib.IL
{
    public interface IEmitSyntaxPluginManager
    {
        bool TryGetPlugin(Type pluginType, out IEmitSyntaxPlugin plugin);

        void Add(Type contract, IEmitSyntaxPlugin plugin);
    }
}
