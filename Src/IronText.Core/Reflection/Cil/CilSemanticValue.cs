using IronText.Framework;
using IronText.Lib.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronText.Reflection.Managed
{
    public class CilSemanticValue
    {
        private readonly MethodInfo[] path;

        public CilSemanticValue(Type type, IEnumerable<MethodInfo> path)
        {
            this.ValueType = type;
            this.path = path.ToArray();
        }

        /// <summary>
        /// Runtime type of the semantic value
        /// </summary>
        public Type ValueType { get; private set; }

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
