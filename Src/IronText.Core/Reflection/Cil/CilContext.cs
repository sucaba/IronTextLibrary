using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public class CilContext
    {
        private readonly MethodInfo[] path;

        public CilContext(Type contextType, IEnumerable<MethodInfo> path)
        {
            this.ContextType = contextType;
            this.path = path.ToArray();
        }

        public string UniqueName
        {
            get { return CilContextRef.GetName(ContextType); }
        }

        /// <summary>
        /// Context type
        /// </summary>
        public Type ContextType { get; private set; }

        public EmitSyntax Ld(EmitSyntax emit, Pipe<EmitSyntax> ldFrom)
        {
            // Load start value
            emit = ldFrom(emit);

            foreach (var getter in path)
            {
                emit = emit.Call(getter);
            }

            return emit;
        }
    }
}
