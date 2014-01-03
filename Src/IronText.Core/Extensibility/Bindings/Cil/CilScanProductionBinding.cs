using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Extensibility.Bindings.Cil
{
    public class CilScanProductionBinding : IScanProductionBinding
    {
        public CilScanProductionBinding()
        {
        }

        public CilScanProductionBinding(ScanActionBuilder builder)
            : this((MethodInfo)null, builder)
        {
        }

        public CilScanProductionBinding(MethodInfo definingMethod, ScanActionBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (definingMethod != null)
            {
                this.ResultTokenType = definingMethod.ReturnType;
                this.DefiningMethod = definingMethod;
            }

            this.Builder        = builder;
        }

        internal CilScanProductionBinding(Type resultTokenType)
        {
            this.ResultTokenType = resultTokenType;
            this.DefiningMethod = null;
        }

        public MethodInfo DefiningMethod { get; private set; }

        public ScanActionBuilder Builder { get; private set; }

        internal Type ResultTokenType { get; private set; }
    }
}
