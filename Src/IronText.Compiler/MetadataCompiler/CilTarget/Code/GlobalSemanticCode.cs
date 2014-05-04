using IronText.Framework;
using IronText.Lib.IL;
using IronText.Reflection;
using IronText.Reflection.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.MetadataCompiler
{
    class GlobalSemanticCode : ISemanticCode
    {
        private EmitSyntax                 emit;
        private readonly Pipe<EmitSyntax>  ldGlobalScope;
        private readonly SemanticScope     globals;

        public GlobalSemanticCode(
            EmitSyntax        emit,
            Pipe<EmitSyntax>  ldGlobalScope,
            SemanticScope     globals)
        {
            this.emit           = emit;
            this.ldGlobalScope  = ldGlobalScope;
            this.globals        = globals;
        }

        public bool LdSemantic(SemanticRef reference)
        {
            var value = globals.Resolve(reference);
            if (value == null)
            {
                return false;
            }

            var binding = value.Joint.The<CilSemanticValue>();
            this.emit = binding.Ld(emit, ldGlobalScope);
            return true;
        }
    }
}
