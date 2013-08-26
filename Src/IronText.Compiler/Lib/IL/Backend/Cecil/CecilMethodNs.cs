using IronText.Framework;
using IronText.Lib.Shared;
using Mono.Cecil;
using SR = System.Reflection;

namespace IronText.Lib.IL.Backend.Cecil
{
    class CecilMethodNs 
        : IMethodNs
        , WantMethodSig
        , WantCallConvSig
        , WantCallKindSig
        , WantReturnTypeSig
        , WantDeclTypeSig
        , WantNameSig
        , WantOpenArgsSig
        , WantArgsSigBase
        , WantArgsSig
        , WantMoreArgsSig
        , DoneMethodSig
    {
        private readonly ModuleDefinition container;
        private MethodReference currentMethodReference;
        private ParameterAttributes currentParameterAttributes;

        public CecilMethodNs(ModuleDefinition container)
        {
            this.container = container;
        }

        public WantMethodSig StartSignature 
        { 
            get 
            { 
                this.currentMethodReference = new MethodReference("Unknown", container.TypeSystem.Void);
                return this;
            }
        }

        public Ref<Methods> Import(SR.MethodBase method)
        {
            var methodRef = container.Import(method);
            var def = new ValueSlot<Methods> { Value = methodRef };
            return def.GetRef();
        }

        public Ref<Methods> Method(Pipe<IMethodNs,DoneMethodSig> code)
        {
            code(this);

            var result = ValueToRef(container.Import(this.currentMethodReference));
            this.currentMethodReference = null;
            return result;
        }

        WantCallConvSig WantCallConvThen<WantCallConvSig>.Instance
        {
            get { currentMethodReference.HasThis = true; return this; }
        }

        WantCallConvSig WantCallConvThen<WantCallConvSig>.Explicit
        {
            get { currentMethodReference.ExplicitThis = true; return this; }
        }

        WantCallKindSig WantCallKindThen<WantCallKindSig>.Default
        {
            get { currentMethodReference.CallingConvention |= MethodCallingConvention.Default; return this; }
        }

        WantCallKindSig WantCallKindThen<WantCallKindSig>.VarArg
        {
            get { currentMethodReference.CallingConvention |= MethodCallingConvention.VarArg; return this;  }
        }

        WantCallKindSig WantCallKindThen<WantCallKindSig>.Cdecl
        {
            get { currentMethodReference.CallingConvention |= MethodCallingConvention.C; return this; }
        }

        WantCallKindSig WantCallKindThen<WantCallKindSig>.StdCall
        {
            get { currentMethodReference.CallingConvention |= MethodCallingConvention.StdCall; return this; }
        }

        WantCallKindSig WantCallKindThen<WantCallKindSig>.ThisCall
        {
            get { currentMethodReference.CallingConvention |= MethodCallingConvention.ThisCall; return this; }
        }

        WantCallKindSig WantCallKindThen<WantCallKindSig>.FastCall
        {
            get { currentMethodReference.CallingConvention |= MethodCallingConvention.FastCall; return this; }
        }

        WantDeclTypeSig WantReturnTypeThen<WantDeclTypeSig>.Returning(Ref<Types> resultType)
        {
            currentMethodReference.ReturnType = (TypeReference)resultType.Value;
            return this;
        }

        WantNameSig WantDeclTypeSig.DecaringType(TypeSpec typeSpec)
        {
            currentMethodReference.DeclaringType = (TypeReference)typeSpec.Type.Value;
            return this;
        }

        WantOpenArgsSig WantNameThen<WantOpenArgsSig>.Named(string methodName)
        {
            currentMethodReference.Name = methodName;
            return this;
        }

        WantArgsSig WantOpenArgsSig.BeginArgs() { return this; }

        DoneMethodSig WantArgsSigBase.EndArgs() { return this; }

        WantArgsSig ParamAttrSyntax1<WantArgsSig>.In
        {
            get 
            { 
                this.currentParameterAttributes |= ParameterAttributes.In;
                return this;
            }
        }

        WantArgsSig ParamAttrSyntax1<WantArgsSig>.Out
        {
            get 
            { 
                this.currentParameterAttributes |= ParameterAttributes.Out;
                return this;
            }
        }

        WantArgsSig ParamAttrSyntax1<WantArgsSig>.Opt
        {
            get
            { 
                this.currentParameterAttributes |= ParameterAttributes.Optional;
                return this;
            }
        }

        public WantMoreArgsSig Argument(Ref<Types> type, string argName)
        {
            var parameter = new ParameterDefinition(
                                argName,
                                currentParameterAttributes,
                                (TypeReference)type.Value);
            this.currentParameterAttributes = ParameterAttributes.None;
            this.currentMethodReference.Parameters.Add(parameter);

            return this;
        }


        WantMoreArgsSig ParamAttrSyntax1<WantMoreArgsSig>.In
        {
            get 
            { 
                this.currentParameterAttributes |= ParameterAttributes.In;
                return this;
            }
        }

        WantMoreArgsSig ParamAttrSyntax1<WantMoreArgsSig>.Out
        {
            get 
            { 
                this.currentParameterAttributes |= ParameterAttributes.Out;
                return this;
            }
        }

        WantMoreArgsSig ParamAttrSyntax1<WantMoreArgsSig>.Opt
        {
            get
            { 
                this.currentParameterAttributes |= ParameterAttributes.Optional;
                return this;
            }
        }
        
        Ref<Methods> ValueToRef(MethodReference methodRef)
        {
            var def = new ValueSlot<Methods>();
            def.Value = methodRef;
            return def.GetRef();
        }
    }
}
