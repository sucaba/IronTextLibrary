using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    public class CilScanProductionDef
    {
        public CilScanProductionDef()
        {
        }

        public CilScanProductionDef(CilScanActionBuilder builder)
            : this((MethodInfo)null, builder)
        {
        }

        public CilScanProductionDef(MethodInfo definingMethod, CilScanActionBuilder builder)
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

        internal CilScanProductionDef(Type resultTokenType)
        {
            this.ResultTokenType = resultTokenType;
            this.DefiningMethod = null;
        }

        public MethodInfo DefiningMethod { get; private set; }

        public CilScanActionBuilder Builder { get; private set; }

        internal Type ResultTokenType { get; private set; }
    }
}
