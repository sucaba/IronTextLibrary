using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection.Managed
{
    public abstract class CilContextRef
    {
        public static string GetName(Type type)
        {
            return type.AssemblyQualifiedName;
        }
        
        public static readonly CilContextRef None = new NoneContextRef();

        public static CilContextRef ThisToken(Type type) { return new ThisContextRef(type); }

        public static CilContextRef ByType(Type type)    { return new ByTypeContextRef(type); }

        public abstract string UniqueName { get; }

        public abstract void LoadForProduction(IProductionActionCode code);

        public abstract void LoadForMerger(IMergeActionCode code);

        public abstract void LoadForMatcher(IMatcherActionCode code);

        sealed class NoneContextRef : CilContextRef
        {
            public override string UniqueName { get { return "$none"; } }

            public override void LoadForProduction(IProductionActionCode code) { }

            public override void LoadForMerger(IMergeActionCode code) { }

            public override void LoadForMatcher(IMatcherActionCode code) { }

            public override bool Equals(object obj)
            {
                return obj is NoneContextRef;
            }

            public override int GetHashCode() { return 0; }
        }

        sealed class ThisContextRef : CilContextRef
        {
            private readonly Type type;

            public ThisContextRef(Type type) { this.type = type; }

            public override string UniqueName { get { return GetName(type); } }

            public override void LoadForProduction(IProductionActionCode code)
            {
                code.LdRuleArg(0, type);
            }

            public override void LoadForMerger(IMergeActionCode code)
            {
                throw new InvalidOperationException("Internal error: this-context is not supported in merge actions.");
            }

            public override void LoadForMatcher(IMatcherActionCode code)
            {
                throw new InvalidOperationException("Internal error: this-context is not supported in merge actions.");
            }

            public override bool Equals(object obj)
            {
                var casted = obj as ThisContextRef;
                return casted != null && type == casted.type;
            }

            public override int GetHashCode()
            {
                return UniqueName.GetHashCode();
            }
        }

        sealed class ByTypeContextRef : CilContextRef
        {
            private readonly Type type;

            public ByTypeContextRef(Type type) { this.type = type; }

            public override string UniqueName { get { return GetName(type); } }

            public override void LoadForProduction(IProductionActionCode code)
            {
                code.ContextResolver.LdContext(UniqueName);
            }

            public override void LoadForMerger(IMergeActionCode code)
            {
                code.ContextResolver.LdContext(UniqueName);
            }

            public override void LoadForMatcher(IMatcherActionCode code)
            {
                code.ContextResolver.LdContext(UniqueName);
            }

            public override bool Equals(object obj)
            {
                var casted = obj as ByTypeContextRef;
                return casted != null && type == casted.type;
            }

            public override int GetHashCode()
            {
                return UniqueName.GetHashCode();
            }
        }
    }
}
