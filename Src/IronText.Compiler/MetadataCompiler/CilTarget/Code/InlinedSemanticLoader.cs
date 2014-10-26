using IronText.Algorithm;
using IronText.Compilation;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler
{
    class InlinedSemanticLoader : ISemanticLoader
    {
        private readonly Fluent<EmitSyntax> emitCoder;
        private readonly ISemanticLoader    globals;
        private readonly Production         parentProduction;
        private readonly int                parentPosition;
        private readonly Production         childProduction;
        private readonly VarsStack          varsStack;
        private int varsStackStart;

        public InlinedSemanticLoader(
            Fluent<EmitSyntax> emitCoder,
            ISemanticLoader    globals,
            VarsStack          varsStack,
            int                varsStackStart,
            Production         parentProduction,
            int                indexInParent,
            Production         childProduction)
        {
            this.emitCoder         = emitCoder;
            this.globals 		   = globals;
            this.varsStack         = varsStack;
            this.varsStackStart    = varsStackStart;
            this.parentProduction  = parentProduction;
            this.parentPosition    = indexInParent;
            this.childProduction   = childProduction;
        }

        public bool LdSemantic(SemanticRef reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            if (reference == SemanticRef.None)
            {
                return true;
            }

            var components = parentProduction.ChildComponents;
            if (parentPosition != 0 && parentPosition != components.Length && components.Length != 0)
            {
                var providingSymbol = (components[0] as Symbol) ?? ((Production)components[0]).Outcome;
                if (providingSymbol != null)
                {
                    if (providingSymbol.LocalScope.Lookup(reference))
                    {
                        SemanticValue    val        = providingSymbol.LocalScope.Resolve(reference);
                        CilSemanticValue valBinding = val.Joint.The<CilSemanticValue>();

                        int slotPos = varsStackStart - parentPosition;
                        emitCoder(il => valBinding
                            .Ld(il, il2 => 
                                {
                                    varsStack.LdSlot(slotPos);
                                    return il2;
                                }));
                        
                        return true;
                    }
                }
            }

            return globals.LdSemantic(reference);
        }
    }
}
