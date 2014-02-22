using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection.Managed
{
    public abstract class CilContext
    {
        public static readonly CilContext None = new NoneContext();

        public static CilContext ThisToken(Type type) { return new ThisTokenContext(type); }

        public static CilContext ByType(Type type)    { return new ByTypeContext(type); }

        public abstract void Load(IProductionActionCode code);

        sealed class NoneContext : CilContext
        {
            public override void Load(IProductionActionCode code) { }
        }

        sealed class ThisTokenContext : CilContext
        {
            private readonly Type type;

            public ThisTokenContext(Type type) { this.type = type; }

            public override void Load(IProductionActionCode code)
            {
                code.LdRuleArg(0, type);
            }
        }

        sealed class ByTypeContext : CilContext
        {
            private readonly Type type;

            public ByTypeContext(Type type) { this.type = type; }

            public override void Load(IProductionActionCode code)
            {
                code.ContextResolver.LdContextOfType(type);
            }
        }
    }
}
