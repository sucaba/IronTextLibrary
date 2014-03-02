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

        public abstract string UniqueName { get; }

        public abstract void Load(IProductionActionCode code);

        public abstract CilContextConsumer GetConsumer();

        sealed class NoneContext : CilContext
        {
            public override string UniqueName { get { return "$none"; } }

            public override void Load(IProductionActionCode code) { }

            public override CilContextConsumer GetConsumer()
            {
                throw new InvalidOperationException("internal error: None context has has no consumer.");
            }
        }

        sealed class ThisTokenContext : CilContext
        {
            private readonly Type type;

            public ThisTokenContext(Type type) { this.type = type; }

            public override string UniqueName { get { return type.AssemblyQualifiedName; } }

            public override void Load(IProductionActionCode code)
            {
                code.LdRuleArg(0, type);
            }

            public override CilContextConsumer GetConsumer()
            {
                return new CilContextConsumer(type);
            }
        }

        sealed class ByTypeContext : CilContext
        {
            private readonly Type type;

            public ByTypeContext(Type type) { this.type = type; }

            public override string UniqueName { get { return type.AssemblyQualifiedName; } }

            public override void Load(IProductionActionCode code)
            {
                code.ContextResolver.LdContextOfType(type);
            }

            public override CilContextConsumer GetConsumer()
            {
                return new CilContextConsumer(type);
            }
        }
    }
}
